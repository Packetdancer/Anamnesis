﻿// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows;
	using Anamnesis.Core.Memory;
	using Anamnesis.GUI.Dialogs;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Anamnesis.WpfStyles;
	using FontAwesome.Sharp;
	using PropertyChanged;

	public delegate void SelectionEvent(ActorViewModel? actor);

	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public class TargetService : ServiceBase<TargetService>
	{
		public static event SelectionEvent? ActorSelected;

		public ActorViewModel? SelectedActor { get; private set; }
		public ObservableCollection<ActorTableActor> PinnedActors { get; set; } = new ObservableCollection<ActorTableActor>();

		public static async Task PinActor(ActorTableActor actor)
		{
			foreach (ActorTableActor otherActor in Instance.PinnedActors)
			{
				if (actor.Pointer == otherActor.Pointer)
				{
					return;
				}
			}

			// Mannequins and housing NPC's get actor type changed, but squadron members do not.
			if (!TerritoryService.GetIsBarracks() && actor.Kind == ActorTypes.EventNpc)
			{
				bool? result = await GenericDialog.Show($"The Actor: \"{actor.Name}\" appears to be a humanoid NPC. Do you want to change them to a player to allow for posing and appearance changes?", "Actor Selection", MessageBoxButton.YesNo);
				if (result == true)
				{
					ActorViewModel? vm = actor.GetViewModel();
					if (vm == null)
						return;

					vm.Nickname = vm.Name + " (" + vm.ObjectKind + ")";

					vm.ObjectKind = ActorTypes.Player;
					await vm.RefreshAsync();

					if (vm.ModelType != 0)
					{
						vm.ModelType = 0;
						await vm.RefreshAsync();
					}

					actor.Model = (Actor)vm.Model!;
				}
			}

			// Carbuncles get model type set to player (but not actor type!)
			if (actor.Kind == ActorTypes.BattleNpc)
			{
				if (actor.ModelType == 409 || actor.ModelType == 410 || actor.ModelType == 412)
				{
					bool? result = await GenericDialog.Show($"The Actor: \"{actor.Name}\" appears to be a Carbuncle. Do you want to change them to a player to allow for posing and appearance changes?", "Actor Selection", MessageBoxButton.YesNo);
					if (result == true)
					{
						ActorViewModel? vm = actor.GetViewModel();
						if (vm == null)
							return;

						vm.ModelType = 0;
						await vm.RefreshAsync();

						actor.Model = (Actor)vm.Model!;
					}
				}
			}

			App.Current.Dispatcher.Invoke(() => Instance.PinnedActors.Add(actor));

			if (Instance.SelectedActor == null)
			{
				Instance.SelectActor(actor);
			}
		}

		public static void UnpinActor(ActorTableActor actor)
		{
			Instance.PinnedActors.Remove(actor);

			if (actor.GetViewModel() == Instance.SelectedActor)
			{
				if (Instance.PinnedActors.Count > 0)
				{
					Instance.SelectActor(Instance.PinnedActors[0]);
				}
				else
				{
					Instance.SelectActorViewModel(null);
				}
			}
		}

		public static List<ActorTableActor> GetActors()
		{
			List<ActorTableActor> actorPointers = new List<ActorTableActor>();

			int count = 0;
			IntPtr startAddress;

			if (GposeService.Instance.IsGpose)
			{
				count = MemoryService.Read<int>(AddressService.GPoseActorTable);
				startAddress = AddressService.GPoseActorTable + 8;
				////ingameTargetAddress = MemoryService.ReadPtr(AddressService.GPoseTargetManager);
			}
			else
			{
				// why 424?
				count = 424;
				startAddress = AddressService.ActorTable;
				////ingameTargetAddress = MemoryService.ReadPtr(AddressService.TargetManager);
			}

			List<ActorTableActor> results = new List<ActorTableActor>();
			for (int i = 0; i < count; i++)
			{
				IntPtr ptr = MemoryService.ReadPtr(startAddress + (i * 8));

				if (ptr == IntPtr.Zero)
					continue;

				Actor actor = MemoryService.Read<Actor>(ptr);

				if (actor.ObjectKind != ActorTypes.Player
						&& actor.ObjectKind != ActorTypes.BattleNpc
						&& actor.ObjectKind != ActorTypes.EventNpc
						&& actor.ObjectKind != ActorTypes.Companion
						&& actor.ObjectKind != ActorTypes.Retainer)
					continue;

				if (string.IsNullOrEmpty(actor.Name))
					continue;

				results.Add(new ActorTableActor(actor, ptr));
			}

			return results;
		}

		public override async Task Start()
		{
			await base.Start();

			List<ActorTableActor> actors = TargetService.GetActors();

			if (actors.Count <= 0)
				return;

			await PinActor(actors[0]);
			this.SelectActor(actors[0]);
		}

		public void ClearSelection()
		{
			App.Current.Dispatcher.Invoke(() =>
			{
				this.SelectedActor = null;
				this.PinnedActors.Clear();
			});
		}

		public Task Retarget()
		{
			App.Current.Dispatcher.Invoke(() =>
			{
				this.SelectedActor = null;
			});

			if (this.PinnedActors.Count > 0)
			{
				foreach (ActorTableActor actor in this.PinnedActors)
				{
					actor.Clear();
				}

				this.SelectActor(this.PinnedActors[0]);
			}

			return Task.CompletedTask;
		}

		public void SelectActor(ActorTableActor actor)
		{
			this.SelectActorViewModel(actor.GetViewModel());

			foreach (ActorTableActor ac in this.PinnedActors)
			{
				ac.SelectionChanged();
			}
		}

		public void SelectActorViewModel(ActorViewModel? actor)
		{
			this.SelectedActor = actor;
			ActorSelected?.Invoke(actor);
		}

		[AddINotifyPropertyChangedInterface]
		public class ActorTableActor : INotifyPropertyChanged
		{
			private ActorViewModel? viewModel;

			public ActorTableActor(Actor actor, IntPtr pointer)
			{
				this.Model = actor;
				this.Pointer = pointer;
				this.IsValid = true;
				this.Initials = string.Empty;

				this.UpdateInitials(actor.Name);
			}

			public event PropertyChangedEventHandler? PropertyChanged;

			public Actor Model { get; set; }
			public string Id => this.Model.Id;
			public IntPtr? Pointer { get; private set; }
			public string Name => this.Model.Name;
			public ActorTypes Kind => this.Model.ObjectKind;
			public IconChar Icon => this.Model.ObjectKind.GetIcon();
			public int ModelType => this.Model.ModelType;
			public string Initials { get; private set; }
			public bool IsValid { get; private set; }
			public bool IsPinned => TargetService.Instance.PinnedActors.Contains(this);

			public bool IsSelected
			{
				get
				{
					return TargetService.Instance.SelectedActor?.Pointer == this.Pointer;
				}

				set
				{
					if (value)
					{
						TargetService.Instance.SelectActor(this);
					}
				}
			}

			public void SelectionChanged()
			{
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsSelected)));
			}

			/// <summary>
			/// Compares actor identity, does not compare pointers.
			/// </summary>
			public bool Is(ActorTableActor other)
			{
				// TODO: Handle cases where multiple actors share a name, but are different actors
				// perhaps compare modelType and customize values?
				return this.Id == other.Id;
			}

			public void Clear()
			{
				this.viewModel = null;
				this.Pointer = null;
			}

			public ActorViewModel? GetViewModel()
			{
				if (this.Pointer == null)
					this.Retarget();

				if (this.viewModel == null || this.Pointer != this.viewModel.Pointer || this.Name != this.viewModel.Name)
					this.Retarget();

				return this.viewModel;
			}

			public override bool Equals(object? obj)
			{
				return obj is ActorTableActor actor &&
					   EqualityComparer<IntPtr?>.Default.Equals(this.Pointer, actor.Pointer) &&
					   this.Name == actor.Name;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(this.Pointer, this.Name);
			}

			private void Retarget()
			{
				if (this.viewModel != null)
					this.viewModel.PropertyChanged += this.OnViewModelPropertyChanged;

				this.viewModel = null;

				List<ActorTableActor> actors = TargetService.GetActors();

				foreach (ActorTableActor actor in actors)
				{
					if (actor.Name == this.Name && actor.Pointer != null)
					{
						ActorViewModel vm = new ActorViewModel((IntPtr)actor.Pointer);

						// Handle case where multiple actor table entries point ot the same actor, but
						// its not the actor we actually want.
						if (vm.Id != this.Id)
							continue;

						this.Pointer = actor.Pointer;
						this.Model = actor.Model;
						this.viewModel = vm;
						this.viewModel.PropertyChanged += this.OnViewModelPropertyChanged;
					}
				}

				this.IsValid = this.viewModel != null;
			}

			private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (this.viewModel != null && e.PropertyName == nameof(ActorViewModel.DisplayName))
				{
					this.UpdateInitials(this.viewModel.DisplayName);
				}
			}

			private void UpdateInitials(string name)
			{
				if (name.Length <= 4)
				{
					this.Initials = name;
				}
				else
				{
					this.Initials = string.Empty;

					string[] parts = name.Split(' ');
					foreach (string part in parts)
					{
						this.Initials += part[0] + ".";
					}

					this.Initials = this.Initials.Trim('.');
				}
			}
		}
	}
}

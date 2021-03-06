﻿// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public class PoseService : ServiceBase<PoseService>
	{
		private NopHookViewModel? freezeRot1;
		private NopHookViewModel? freezeRot2;
		private NopHookViewModel? freezeRot3;
		private NopHookViewModel? freezeScale1;
		private NopHookViewModel? freezePosition;
		private NopHookViewModel? freezePosition2;
		private NopHookViewModel? freeseScale2;
		private NopHookViewModel? freezePhysics1;
		private NopHookViewModel? freezePhysics2;
		private NopHookViewModel? freezePhysics3;

		private bool isEnabled;

		public delegate void PoseEvent(bool value);

		public static event PoseEvent? EnabledChanged;

		public bool IsEnabled
		{
			get
			{
				return this.isEnabled;
			}

			set
			{
				if (this.IsEnabled == value)
					return;

				this.SetEnabled(value);
			}
		}

		public bool FreezePhysics
		{
			get
			{
				return this.freezePhysics1?.Enabled ?? false;
			}
			set
			{
				this.FreezePositions = value;
				this.FreezeScale = value;

				this.freezePhysics1?.SetEnabled(value);
				this.freezePhysics2?.SetEnabled(value);
			}
		}

		public bool FreezePositions
		{
			get
			{
				return this.freezePosition?.Enabled ?? false;
			}
			set
			{
				this.freezePosition?.SetEnabled(value);
				this.freezePosition2?.SetEnabled(value);
			}
		}

		public bool FreezeScale
		{
			get
			{
				return this.freezeScale1?.Enabled ?? false;
			}
			set
			{
				this.freezeScale1?.SetEnabled(value);
				this.freezePhysics3?.SetEnabled(value);
				this.freeseScale2?.SetEnabled(value);
			}
		}

		public bool FreezeRotation
		{
			get
			{
				return this.freezeRot1?.Enabled ?? false;
			}
			set
			{
				this.freezeRot1?.SetEnabled(value);
				this.freezeRot2?.SetEnabled(value);
				this.freezeRot3?.SetEnabled(value);
			}
		}

		public bool CanEdit { get; set; }

		public override async Task Initialize()
		{
			await base.Initialize();

			this.freezePosition = new NopHookViewModel(AddressService.SkeletonFreezePosition, 5);
			this.freezePosition2 = new NopHookViewModel(AddressService.SkeletonFreezePosition2, 5);
			this.freezeRot1 = new NopHookViewModel(AddressService.SkeletonFreezeRotation, 6);
			this.freezeRot2 = new NopHookViewModel(AddressService.SkeletonFreezeRotation2, 6);
			this.freezeRot3 = new NopHookViewModel(AddressService.SkeletonFreezeRotation3, 4);
			this.freezeScale1 = new NopHookViewModel(AddressService.SkeletonFreezeScale, 6);
			this.freeseScale2 = new NopHookViewModel(AddressService.SkeletonFreezeScale2, 6);
			this.freezePhysics1 = new NopHookViewModel(AddressService.SkeletonFreezePhysics, 4);
			this.freezePhysics2 = new NopHookViewModel(AddressService.SkeletonFreezePhysics2, 3);
			this.freezePhysics3 = new NopHookViewModel(AddressService.SkeletonFreezePhysics3, 4);

			GposeService.GposeStateChanging += this.OnGposeStateChanging;
		}

		public override async Task Shutdown()
		{
			await base.Shutdown();
			this.SetEnabled(false);
		}

		public void SetEnabled(bool enabled)
		{
			// Don't try to enable posing unless we are in gpose
			if (enabled && !GposeService.Instance.IsGpose)
				throw new Exception("Attempt to enable posing outside of gpose");

			this.isEnabled = enabled;

			////this.FreezePositions = enabled;
			this.FreezePhysics = enabled;
			this.FreezeRotation = enabled;
			////this.FreezeScale = enabled;

			EnabledChanged?.Invoke(enabled);

			this.RaisePropertyChanged(nameof(this.IsEnabled));
		}

		private void OnGposeStateChanging()
		{
			this.SetEnabled(false);
		}
	}
}

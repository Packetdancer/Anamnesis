﻿// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public class TerritoryService : ServiceBase<TerritoryService>
	{
		private ushort currentWeatherId;

		public int CurrentTerritoryId { get; private set; }
		public string CurrentTerritoryName { get; private set; } = "Unknown";
		public ITerritoryType? CurrentTerritory { get; private set; }

		public ushort CurrentWeatherId
		{
			get
			{
				return this.currentWeatherId;
			}
			set
			{
				if (this.currentWeatherId == value)
					return;

				this.currentWeatherId = value;
				this.CurrentWeather = null;
				if (this.CurrentTerritory != null)
				{
					foreach (IWeather weather in this.CurrentTerritory.Weathers)
					{
						if (weather.WeatherId == value)
						{
							this.CurrentWeather = weather;
						}
					}
				}
			}
		}

		public IWeather? CurrentWeather { get; set; }

		public static bool GetIsBarracks()
		{
			// Twin adder barracks = 534
			// Immortal Flame barracks = 535
			// Maelstrom barracks = 536
			if (Instance.CurrentTerritoryId >= 534 && Instance.CurrentTerritoryId <= 536)
				return true;

			return false;
		}

		public override async Task Start()
		{
			await base.Start();

			_ = Task.Run(this.Update);

			this.PropertyChanged += this.OnThisPropertyChanged;
		}

		private async Task Update()
		{
			while (this.IsAlive)
			{
				await Task.Delay(10);

				// Update territory
				int newTerritoryId = MemoryService.Read<int>(AddressService.Territory);

				if (newTerritoryId != this.CurrentTerritoryId)
				{
					this.CurrentTerritoryId = newTerritoryId;

					if (GameDataService.Territories == null)
					{
						this.CurrentTerritoryName = $"Unkown ({this.CurrentTerritoryId})";
					}
					else
					{
						this.CurrentTerritory = GameDataService.Territories.Get(this.CurrentTerritoryId);
						this.CurrentTerritoryName = this.CurrentTerritory?.Region + " - " + this.CurrentTerritory?.Place;
					}
				}

				// Update weather
				ushort weatherId;
				if (GposeService.Instance.IsGpose)
				{
					weatherId = MemoryService.Read<ushort>(AddressService.GPoseWeather);
				}
				else
				{
					weatherId = MemoryService.Read<byte>(AddressService.Weather);
				}

				if (weatherId != this.CurrentWeatherId)
				{
					this.CurrentWeatherId = weatherId;
				}
			}
		}

		private void OnThisPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(TerritoryService.CurrentWeatherId))
			{
				ushort current;
				if (GposeService.Instance.IsGpose)
				{
					current = MemoryService.Read<ushort>(AddressService.GPoseWeather);

					if (current != this.CurrentWeatherId)
					{
						MemoryService.Write<ushort>(AddressService.GPoseWeather, this.CurrentWeatherId);
					}
				}
				else
				{
					current = MemoryService.Read<byte>(AddressService.Weather);

					if (current != this.CurrentWeatherId)
					{
						MemoryService.Write<byte>(AddressService.Weather, (byte)this.CurrentWeatherId);
					}
				}
			}
			else if (e.PropertyName == nameof(TerritoryService.CurrentWeather))
			{
				if (this.CurrentWeather == null)
					return;

				if (this.CurrentWeatherId == this.CurrentWeather.WeatherId)
					return;

				this.CurrentWeatherId = this.CurrentWeather.WeatherId;
			}
		}
	}
}

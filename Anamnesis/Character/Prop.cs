﻿// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Character
{
	using System.Windows.Media;
	using Anamnesis.GameData;

	public class Prop : IItem
	{
		public int Key { get; set; }

		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public ushort ModelBase { get; set; }
		public ushort ModelVariant { get; set; }
		public ushort ModelSet { get; set; }

		public bool IsWeapon => true;
		public bool HasSubModel => false;
		public ushort SubModelBase => 0;
		public ushort SubModelVariant => 0;
		public ushort SubModelSet => 0;
		public ImageSource? Icon => null;
		public Classes EquipableClasses => Classes.All;

		public bool FitsInSlot(ItemSlots slot)
		{
			return slot == ItemSlots.MainHand || slot == ItemSlots.OffHand;
		}
	}
}

﻿// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Windows.Documents;
	using PropertyChanged;

	[StructLayout(LayoutKind.Explicit)]
	public struct Bones
	{
		////[FieldOffset(0x00)]
		////public IntPtr HkAnimationFile;

		[FieldOffset(0x10)]
		public int Count;

		[FieldOffset(0x18)]
		public IntPtr TransformArray;
	}

	public class BonesViewModel : MemoryViewModelBase<Bones>
	{
		public BonesViewModel(IntPtr pointer, IStructViewModel? parent = null)
			: base(pointer, parent)
		{
		}

		public BonesViewModel(IMemoryViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		[ModelField] public int Count { get; set; }
		[ModelField] public IntPtr TransformArray { get; set; }

		public List<TransformViewModel> Transforms { get; set; } = new List<TransformViewModel>();

		protected override bool HandleModelToViewUpdate(PropertyInfo viewModelProperty, FieldInfo modelField)
		{
			bool changed = base.HandleModelToViewUpdate(viewModelProperty, modelField);

			if (viewModelProperty.Name == nameof(BonesViewModel.TransformArray) && this.TransformArray != IntPtr.Zero)
			{
				// If we think there are more than 512 bones its likely that
				// this is actually an invalid pointer that we got from memory.
				if (this.Count > 512)
					return changed;

				if (this.Count <= 0)
				{
					this.Transforms.Clear();
				}
				else
				{
					if (this.Transforms.Count != this.Count)
					{
						this.Transforms.Clear();

						IntPtr ptr = this.TransformArray;
						for (int i = 0; i < this.Count; i++)
						{
							this.Transforms.Add(new TransformViewModel(ptr));
							ptr += 0x30;
						}
					}
				}
			}

			return changed;
		}
	}
}

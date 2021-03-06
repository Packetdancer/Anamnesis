﻿// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using Anamnesis.Memory.Types;
	using Anamnesis.Services;

	public class CameraService : ServiceBase<CameraService>
	{
		private bool delimitCamera;

		public CameraViewModel? Camera { get; set; }

		public bool LockCameraPosition
		{
			get;
			set;
		}

		public bool DelimitCamera
		{
			get
			{
				return this.delimitCamera;
			}
			set
			{
				this.delimitCamera = value;

				if (this.Camera == null)
					return;

				this.Camera.MaxZoom = value ? 256 : 20;
				this.Camera.MinZoom = value ? 0 : 1.75f;
				this.Camera.YMin = value ? 1.5f : 1.25f;
				this.Camera.YMax = value ? -1.5f : -1.4f;
			}
		}

		public Vector CameraPosition { get; set; }

		public override async Task Start()
		{
			await base.Start();

			IntPtr ptr = MemoryService.ReadPtr(AddressService.Camera);
			this.Camera = new CameraViewModel(ptr);

			_ = Task.Run(this.Tick);
		}

		private async Task Tick()
		{
			while (this.IsAlive)
			{
				await Task.Delay(50);

				if (!GposeService.Instance.IsGpose || GposeService.Instance.IsChangingState)
				{
					this.LockCameraPosition = false;
					continue;
				}

				if (this.LockCameraPosition)
				{
					MemoryService.Write(AddressService.GPoseCamera, this.CameraPosition);
				}
				else
				{
					this.CameraPosition = MemoryService.Read<Vector>(AddressService.GPoseCamera);
				}
			}
		}
	}
}

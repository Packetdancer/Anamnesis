﻿// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using Anamnesis.Serialization;

	public class TipService : ServiceBase<TipService>
	{
		private List<TipEntry>? tips;

		public bool IsHydaelyn { get; set; }
		public bool IsZodiark { get; set; }
		public bool IsAmaurotine { get; set; }
		public bool IsAnamTan { get; set; }

		public TipEntry? Tip { get; set; }

		public override Task Start()
		{
			try
			{
				this.tips = SerializerService.DeserializeFile<List<TipEntry>>("Tips.json");
			}
			catch (Exception ex)
			{
				Log.Write(SimpleLog.Severity.Warning, new Exception("Failed to load tips.", ex));
			}

			Task.Run(this.TipCycle);

			return base.Start();
		}

		public void NextTip()
		{
			Random rnd = new Random();

			int portraitId = rnd.Next(0, 3);

			this.IsHydaelyn = portraitId == 0;
			this.IsZodiark = portraitId == 1;
			this.IsAmaurotine = portraitId == 2;
			////this.IsAnamTan = portraitId == 3;

			if (this.tips == null)
			{
				TipEntry tip = new TipEntry();
				tip.Text = "I couldn't find any tips. =(";
				this.Tip = tip;
				return;
			}

			int index = rnd.Next(0, this.tips.Count);
			this.Tip = this.tips[index];
		}

		public void KnowMore()
		{
			if (this.Tip == null)
				return;

			ProcessStartInfo psi = new ProcessStartInfo();
			psi.FileName = this.Tip.Url;
			psi.UseShellExecute = true;

			Process.Start(psi);
		}

		private async Task TipCycle()
		{
			this.NextTip();

			while (this.IsAlive)
			{
				await Task.Delay(30000);
				this.NextTip();
			}
		}

		[Serializable]
		public class TipEntry
		{
			public string? Text { get; set; }
			public string? Url { get; set; }

			public bool CanClick => !string.IsNullOrEmpty(this.Url);
		}
	}
}

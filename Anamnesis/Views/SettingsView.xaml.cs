﻿// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Views
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Controls;
	using Anamnesis.Services;
	using MaterialDesignColors;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for ThemeSettingsView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class SettingsView : UserControl
	{
		public SettingsView()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;

			List<double> sizes = new List<double>();
			sizes.Add(0.5);
			sizes.Add(0.6);
			sizes.Add(0.8);
			sizes.Add(0.9);
			sizes.Add(1.0);
			sizes.Add(1.25);
			sizes.Add(1.5);
			sizes.Add(1.75);
			sizes.Add(2.0);
			this.SizeSelector.ItemsSource = sizes;

			List<LanguageOption> languages = new List<LanguageOption>();

			foreach ((string key, string name) in LocalizationService.GetAvailableLocales())
			{
				languages.Add(new LanguageOption(key, name));
			}

			this.Languages = languages;
		}

		public SwatchesProvider Swatches => new SwatchesProvider();
		public SettingsService SettingsService => SettingsService.Instance;

		public IEnumerable<LanguageOption> Languages { get; }

		public Swatch SelectedSwatch
		{
			get
			{
				foreach (Swatch sw in this.Swatches.Swatches)
				{
					if (sw.Name == SettingsService.Current.ThemeSwatch)
					{
						return sw;
					}
				}

				return this.Swatches.Swatches.First();
			}

			set
			{
				SettingsService.Current.ThemeSwatch = value.Name;
			}
		}

		public LanguageOption SelectedLanguage
		{
			get
			{
				foreach (LanguageOption language in this.Languages)
				{
					if (language.Key == SettingsService.Current.Language)
					{
						return language;
					}
				}

				return this.Languages.First();
			}

			set
			{
				SettingsService.Current.Language = value.Key;
				LocalizationService.SetLocale(value.Key);
			}
		}

		public class LanguageOption
		{
			public LanguageOption(string key, string display)
			{
				this.Key = key;
				this.Display = display;
			}

			public string Key { get; }
			public string Display { get; }
		}
	}
}

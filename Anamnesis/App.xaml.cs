﻿// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Threading;
	using Anamnesis.GUI;
	using Anamnesis.Services;
	using MaterialDesignThemes.Wpf;

	using Application = System.Windows.Application;

	/// <summary>
	/// Interaction logic for App.xaml.
	/// </summary>
	public partial class App : Application
	{
		private static readonly ServiceManager Services = new ServiceManager();

		protected override void OnStartup(StartupEventArgs e)
		{
			AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
			this.Dispatcher.UnhandledException += this.DispatcherOnUnhandledException;
			Application.Current.DispatcherUnhandledException += this.CurrentOnDispatcherUnhandledException;
			TaskScheduler.UnobservedTaskException += this.TaskSchedulerOnUnobservedTaskException;
			this.Exit += this.OnExit;

			base.OnStartup(e);

			this.MainWindow = new SplashWindow();
			this.MainWindow.Show();

			Task.Run(this.Start);
		}

		private void OnExit(object sender, ExitEventArgs e)
		{
			Task.Run(async () => { await Services.ShutdownServices(); });
		}

		private void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
		{
			if (e.Exception == null)
				return;

			Log.Write(e.Exception, @"Unhandled Task", Log.Severity.Critical);
		}

		private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			Log.Write(e.Exception, @"Unhandled Current Dispatcher", Log.Severity.Critical);
		}

		private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			Log.Write(e.Exception, @"Unhandled Dispatcher", Log.Severity.Critical);
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Exception? ex = e.ExceptionObject as Exception;

			if (ex == null)
				return;

			Log.Write(ex, @"Unhandled", Log.Severity.Critical);
		}

		private async Task Start()
		{
			try
			{
				await Services.InitializeServices();

				Application.Current.Dispatcher.Invoke(() =>
				{
					Window oldwindow = this.MainWindow;
					this.MainWindow = new Anamnesis.GUI.MainWindow();
					this.MainWindow.Show();
					oldwindow.Close();
				});
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}
	}
}

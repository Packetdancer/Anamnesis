﻿// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Reflection;
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using Anamnesis;
	using Anamnesis.GUI.Windows;

	#pragma warning disable SA1649

	public delegate void DrawerEvent();
	public delegate void DialogEvent();

	public enum DrawerDirection
	{
		Left,
		Top,
		Right,
		Bottom,
	}

	public interface IDrawer
	{
		event DrawerEvent Close;
	}

	public interface IDialog<TResult> : IDialog
	{
		TResult Result { get; }
	}

	public interface IDialog
	{
		event DialogEvent Close;

		void Cancel();
	}

	public class ViewService : ServiceBase<ViewService>
	{
		public delegate Task DrawerEvent(string? title, UserControl drawer, DrawerDirection direction);

		public static event DrawerEvent? ShowingDrawer;

		public static Task ShowDrawer<T>(string? title = null, DrawerDirection direction = DrawerDirection.Right)
		{
			UserControl? view = CreateView<T>();

			if (view == null || ShowingDrawer == null)
				return Task.CompletedTask;

			return ShowingDrawer.Invoke(title, view, direction);
		}

		public static Task ShowDrawer(object view, string? title = null, DrawerDirection direction = DrawerDirection.Right)
		{
			if (!(view is UserControl control))
				throw new Exception("Invalid view");

			if (ShowingDrawer == null)
				return Task.CompletedTask;

			return ShowingDrawer.Invoke(title, control, direction);
		}

		public static Task<TResult> ShowDialog<TView, TResult>(string title)
			where TView : IDialog<TResult>
		{
			UserControl? userControl = CreateView<TView>();

			if (userControl is TView view)
				return ShowDialog<TView, TResult>(title, view);

			throw new InvalidOperationException();
		}

		public static Task<TResult> ShowDialog<TView, TResult>(string title, TView view)
			where TView : IDialog<TResult>
		{
			Dialog dlg = new Dialog();
			dlg.ContentArea.Content = view;
			dlg.TitleText.Text = title;
			dlg.Owner = App.Current.MainWindow;

			IDialog<TResult>? dialogInterface = dlg.ContentArea.Content as IDialog<TResult>;

			if (dialogInterface == null)
				throw new Exception("Dialog interface not found");

			dialogInterface.Close += () => dlg.Close();

			dlg.ShowDialog();

			return Task.FromResult(dialogInterface.Result);
		}

		private static UserControl? CreateView<T>()
		{
			Type viewType = typeof(T);

			if (!typeof(UserControl).IsAssignableFrom(viewType))
				throw new Exception($"View: {viewType} does not extend from UserControl.");

			UserControl? view;
			try
			{
				view = Activator.CreateInstance(viewType) as UserControl;
			}
			catch (TargetInvocationException ex)
			{
				Log.Write(new Exception($"Failed to create view: {viewType}", ex.InnerException));
				return null;
			}
			catch (Exception ex)
			{
				Log.Write(new Exception($"Failed to create view: {viewType}", ex));
				return null;
			}

			return view;
		}
	}
}

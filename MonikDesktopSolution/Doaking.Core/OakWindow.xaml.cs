using Autofac;
using MahApps.Metro.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Doaking.Core.Oak;

namespace Doaking.Core
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class OakWindow : MetroWindow
	{
		private readonly Shell _shell;

		public OakWindow(Shell aShell)
		{
			_shell = aShell;

			InitializeComponent();
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			var app = _shell.Resolve<IOakApplication>();

			app.WhenAnyValue(x => x.Title)
				.Subscribe(v => this.Title = v);

			_shell.AttachDocker(MainDocker);

			this.DataContext = _shell;
		}

		private void MainDocker_ActiveContentChanged(object sender, EventArgs e)
		{
			var uc = MainDocker.ActiveContent as UserControl;

			if (uc?.DataContext is IDockingWindow)
				_shell.SelectedWindow = (IDockingWindow) uc.DataContext;
		}
	}
}
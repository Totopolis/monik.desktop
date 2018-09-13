using MonikDesktop.Common.Interfaces;
using System.Windows;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;

namespace MonikDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
	{
		public App()
		{
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

		    var shell = UiStarter.Start<MainWindow>(
		        new Bootstrap(),
		        new UiShowStartWindowOptions
		        {
		            Title = "Kanban.Desktop",
		            ToolPaneWidth = 100
		        });

            shell.ShowView<IStartupView>();
		}
	}
}
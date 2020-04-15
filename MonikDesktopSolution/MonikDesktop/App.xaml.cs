using MonikDesktop.Views;
using System.Windows;
using Ui.Wpf.Common;
using Ui.Wpf.Common.DockingManagers;
using Ui.Wpf.Common.ShowOptions;

namespace MonikDesktop
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var shell = UiStarter.Start<IDockWindow>(new Bootstrap(),
                new UiShowStartWindowOptions {Title = "Monik.Desktop"});

            shell.SetContainerWidth(DefaultDockingManager.Tools, new GridLength(350));
            shell.ShowTool<StartupView>(new ViewRequest("startup"));
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using MonikDesktop.Views;
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

            var dm = new DefaultDockingManager
            {
                DocumentPaneControlStyle = FindResource("AvalonDockThemeCustomDocumentPaneControlStyle") as Style,
                AnchorablePaneControlStyle = FindResource("AvalonDockThemeCustomAnchorablePaneControlStyle") as Style,
            };
            dm.SetResourceReference(Control.BackgroundProperty, "MahApps.Brushes.White");

            var shell = UiStarter.Start<IDockWindow>(new Bootstrap(),
                new UiShowStartWindowOptions
                {
                    Title = "Monik.Desktop",
                    DockingManager = dm,
                });

            shell.SetContainerWidth(DefaultDockingManager.Tools, new GridLength(350));
            shell.ShowTool<StartupView>(new ViewRequest("startup"));
        }
    }
}

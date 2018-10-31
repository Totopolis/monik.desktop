using AmmySidekick;
using MonikDesktop.Views;
using System;
using System.Windows;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;

namespace MonikDesktop
{
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            App app = new App();
            app.InitializeComponent();

            RuntimeUpdateHandler.Register(app, "/" + AmmySidekick.Ammy.GetAssemblyName(app) + ";component/App.g.xaml");

            app.Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var shell = UiStarter.Start<IDockWindow>(new Bootstrap(),
                new UiShowStartWindowOptions {Title = "Monik.Desktop"});
            shell.ShowTool<StartupView>(new ViewRequest("startup"));
        }
    }
}

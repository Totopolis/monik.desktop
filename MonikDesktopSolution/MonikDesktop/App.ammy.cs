using AmmySidekick;
using MonikDesktop.Views;
using System;
using System.Windows;
using Ui.Wpf.Common;

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

            var shell = UiStarter.Start<IDockWindow>(new Bootstrap());
            shell.ShowTool<StartupView>();
        }
    }
}

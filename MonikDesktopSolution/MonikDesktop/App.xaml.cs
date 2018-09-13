using MonikDesktop.Common.Interfaces;
using System.Windows;
using Ui.Wpf.Common;

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

            UiStarter.Start<IDockWindow, IStartupView>(new Bootstrap());
        }
    }
}
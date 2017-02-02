using System.Windows;
using Doaking.Core;
using MonikDesktop.Common.Interfaces;

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

			var shell = Bootstrap.Init();

			OakWindow window = new OakWindow(shell);

			var startup = shell.Resolve<IStartupWindow>();
			shell.ShowDocument(startup);

			window.Show();
		}
	}
}
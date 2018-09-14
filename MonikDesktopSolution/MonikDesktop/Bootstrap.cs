using Autofac;
using MonikDesktop.Common;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Properties;
using MonikDesktop.ViewModels;
using MonikDesktop.Views;
using Ui.Wpf.Common;

namespace MonikDesktop
{
	public class Bootstrap : IBootstraper
    {
		public IShell Init()
		{
			var builder = new ContainerBuilder();

		    builder.RegisterType<Shell>().As<IShell>().SingleInstance();
            builder.RegisterType<AppModel>().As<IAppModel>().SingleInstance();
			builder.RegisterType<MainWindow>().As<IDockWindow>().SingleInstance();

			builder.RegisterType<MonikService>().As<IMonikService>();
			builder.RegisterType<SourcesCache>().As<ISourcesCache>().SingleInstance();

            // ViewModels
			builder.RegisterType<LogsViewModel>().As<ILogsViewModel>();
			builder.RegisterType<KeepAliveViewModel>().As<IKeepAliveViewModel>();
			builder.RegisterType<MetricsViewModel>().As<IMetricsViewModel>();
			builder.RegisterType<SourcesViewModel>().As<ISourcesViewModel>().SingleInstance();
			builder.RegisterType<PropertiesViewModel>().As<IPropertiesViewModel>().SingleInstance();
			builder.RegisterType<StartupViewModel>().As<IStartupViewModel>().SingleInstance();
			builder.RegisterType<LogDescriptionViewModel>().As<ILogDescriptionViewModel>().SingleInstance();

            // Views
		    builder.RegisterType<StartupView>().As<IStartupView>();
		    builder.RegisterType<LogsView>().As<ILogsView>();
		    builder.RegisterType<KeepAliveView>().As<IKeepAliveView>();
		    builder.RegisterType<MetricsView>().As<IMetricsView>();
		    builder.RegisterType<PropertiesView>().As<IPropertiesView>();
		    builder.RegisterType<SourcesView>().As<ISourcesView>();
		    builder.RegisterType<LogDescriptionView>().As<ILogDescriptionView>();

            var container = builder.Build();

			var shell = container.Resolve<IShell>();
			shell.Container = container;

			container.Resolve<IAppModel>().ServerUrl = Settings.Default.ServerUrl;

			return shell;
		}
	}
}
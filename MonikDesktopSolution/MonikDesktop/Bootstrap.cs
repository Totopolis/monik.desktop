using Autofac;
using MonikDesktop.Common;
using MonikDesktop.Common.Interfaces;
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
			builder.RegisterType<MainWindow>().As<IDockWindow>().SingleInstance();

			builder.RegisterType<MonikService>().As<IMonikService>();
		    builder.RegisterType<SourcesCache>().As<ISourcesCache>();
		    builder.RegisterType<SourcesCacheProvider>().As<ISourcesCacheProvider>().SingleInstance();

            // Views and ViewModels
		    builder.RegisterType<StartupView>();
		    builder.RegisterType<StartupViewModel>();

		    builder.RegisterType<LogsView>();
            builder.RegisterType<LogsViewModel>();
		    builder.RegisterType<KeepAliveView>();
            builder.RegisterType<KeepAliveViewModel>();
		    builder.RegisterType<MetricsView>();
            builder.RegisterType<MetricsViewModel>();

		    builder.RegisterType<PropertiesView>();
		    builder.RegisterType<PropertiesViewModel>();
            builder.RegisterType<SourcesView>();
            builder.RegisterType<SourcesViewModel>();
		    builder.RegisterType<LogDescriptionView>();
            builder.RegisterType<LogDescriptionViewModel>();

		    builder.RegisterType<RemoveEntitiesView>();
            builder.RegisterType<RemoveEntitiesViewModel>();
		    builder.RegisterType<ManageGroupsView>();
            builder.RegisterType<ManageGroupsViewModel>();
		    
            var container = builder.Build();

			var shell = container.Resolve<IShell>();
			shell.Container = container;

			return shell;
		}
	}
}
using Autofac;
using Doaking.Core.Oak;
using MonikDesktop.Common;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Properties;
using MonikDesktop.ViewModels;
using MonikDesktop.Views;

namespace MonikDesktop
{
	public class Bootstrap
	{
		public static Shell Init()
		{
			var builder = new ContainerBuilder();

			builder.RegisterType<OakApplication>().As<IOakApplication>().SingleInstance();
			builder.RegisterType<Shell>().SingleInstance();
			builder.RegisterType<MonikService>().As<IMonikService>();
			builder.RegisterType<SourcesCache>().As<ISourcesCache>().SingleInstance();
			builder.RegisterType<LogsViewModel>().As<ILogsWindow>();
			builder.RegisterType<SourcesViewModel>().As<ISourcesWindow>().SingleInstance();
			builder.RegisterType<PropertiesViewModel>().As<IPropertiesWindow>().SingleInstance();
			builder.RegisterType<StartupViewModel>().As<IStartupWindow>().SingleInstance();
			builder.RegisterType<LogDescriptionViewModel>().As<ILogDescription>().SingleInstance();

			var container = builder.Build();

			var shell = container.Resolve<Shell>();
			shell.Container = container;

			shell.RegisterModelView<IStartupWindow, StartupView>();
			shell.RegisterModelView<ILogsWindow, LogsView>();
			shell.RegisterModelView<IPropertiesWindow, PropertiesView>();
			shell.RegisterModelView<ISourcesWindow, SourcesView>();
			shell.RegisterModelView<ILogDescription, LogDescriptionView>();

			shell.Resolve<IOakApplication>().ServerUrl = Settings.Default.ServerUrl;

			return shell;
		}
	}
}
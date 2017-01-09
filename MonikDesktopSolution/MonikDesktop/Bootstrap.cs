using Autofac;
using MonikDesktop.Oak;
using MonikDesktop.ViewModels;
using MonikDesktop.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonikDesktop
{
  public class Bootstrap
  {
    public static IContainer Container { get; set; }

    public static void Init()
    {
      var builder = new ContainerBuilder();

      builder.RegisterType<MApp>().SingleInstance();
      builder.RegisterType<Shell>().SingleInstance();
      builder.RegisterType<MonikService>().As<IMonikService>();
      builder.RegisterType<SourcesCache>().As<ISourcesCache>().SingleInstance();
      builder.RegisterType<LogsViewModel>().As<ILogsWindow>();
      builder.RegisterType<SourcesViewModel>().As<ISourcesWindow>().SingleInstance();
      builder.RegisterType<PropertiesViewModel>().As<IPropertiesWindow>().SingleInstance();
      builder.RegisterType<StartupViewModel>().As<IStartupWindow>().SingleInstance();

      Container = builder.Build();

      var _shell = Container.Resolve<Shell>();

      _shell.RegisterModelView<IStartupWindow, StartupView>();
      _shell.RegisterModelView<ILogsWindow, LogsView>();
      _shell.RegisterModelView<IPropertiesWindow, PropertiesView>();
      _shell.RegisterModelView<ISourcesWindow, SourcesView>();

      Container.Resolve<MApp>().ServerUrl = Properties.Settings.Default.ServerUrl;
    }
  }
}

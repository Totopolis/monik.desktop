using Autofac;
using MonikDesktop.ViewModels;
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
      builder.RegisterType<MonikService>().As<IMonikService>();
      builder.RegisterType<SourcesCache>().As<ISourcesCache>().SingleInstance();
      builder.RegisterType<LogsViewModel>().As<ILogsWindow>();
      builder.RegisterType<SourcesViewModel>().As<ISourcesWindow>().SingleInstance();
      builder.RegisterType<PropertiesViewModel>().As<IPropertiesWindow>().SingleInstance();
      builder.RegisterType<StartupViewModel>().As<IStartupWindow>().SingleInstance();

      Container = builder.Build();
    }
  }
}

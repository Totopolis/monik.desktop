using Autofac;
using Caliburn.Micro;
using Gemini.Framework;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonikDesktop.Modules
{
  [Export(typeof(IModule))]
  public class MainModule : ModuleBase
  {
    //[ImportingConstructor]
    public MainModule()
    {
    }

    public override void Initialize()
    {
      Shell.ShowFloatingWindowsInTaskbar = true;
      //Shell.ToolBars.Visible = true;

      MApp _app = Bootstrap.Container.Resolve<MApp>();

      _app.WhenAnyValue(vm => vm.Name)
        .Subscribe(t => MainWindow.Title = t);

      _app.ServerUrl = Properties.Settings.Default.ServerUrl;

      //MainWindow.Icon = Resources.favicon.ToBitmap().ToBitmapImage();

      Shell.MainMenu.RemoveAt(4);
      Shell.MainMenu.RemoveAt(3);
      Shell.MainMenu.RemoveAt(2);
      Shell.MainMenu.RemoveAt(1);
      Shell.MainMenu.RemoveAt(0);
    }

    public override void PostInitialize()
    {
      var _start = Bootstrap.Container.Resolve<IStartupWindow>();
      Shell.ShowTool(_start);
    }
  }
}

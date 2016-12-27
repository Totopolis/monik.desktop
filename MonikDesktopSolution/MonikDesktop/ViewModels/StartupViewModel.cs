using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Framework;
using Gemini.Framework.Services;
using ReactiveUI;
using System.Reactive;
using MonikDesktop;
using Autofac;
using Caliburn.Micro;

namespace MonikDesktop.ViewModels
{
  public class StartupViewModel : Tool, IStartupWindow
  {
    private MApp FApp;

    public MApp App { get { return FApp; } }

    public ReactiveCommand NewLogCommand { get; set; }
    public ReactiveCommand NewKeepAliveCommand { get; set; }

    public StartupViewModel(MApp aApp) : base()
    {
      FApp = aApp;

      DisplayName = "App settings";

      FApp.ObservableForProperty(x => x.ServerUrl)
        .Subscribe(v=>
        {
          Properties.Settings.Default.ServerUrl = v.Value;
          Properties.Settings.Default.Save();
        });

      var _canNew = FApp.WhenAny(x => x.ServerUrl, x => !String.IsNullOrWhiteSpace(x.Value));
      NewLogCommand = ReactiveCommand.Create(NewLog, _canNew);
      NewKeepAliveCommand = ReactiveCommand.Create(NewLog, _canNew);
    }

    protected override void OnViewLoaded(object view)
    {
      base.OnViewLoaded(view);
    }

    protected override void OnInitialize()
    {
      base.OnInitialize();
    }

    private void NewLog()
    {
      var _shell = IoC.Get<IShell>();

      var _log = Bootstrap.Container.Resolve<ILogsWindow>();
      _shell.ShowTool(_log);

      var _sources = Bootstrap.Container.Resolve<ISourcesWindow>();
      _shell.ShowTool(_sources);

      var _props = Bootstrap.Container.Resolve<IPropertiesWindow>();
      _shell.ShowTool(_props);

      FApp.SelectedWindow = _log;
    }

    public override PaneLocation PreferredLocation
    {
      get { return PaneLocation.Right; }
    }
  }
}

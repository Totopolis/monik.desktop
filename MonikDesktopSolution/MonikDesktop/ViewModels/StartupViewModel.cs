using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive;
using MonikDesktop;
using Autofac;
using MonikDesktop.Oak;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop.ViewModels
{
  public class StartupViewModel : ReactiveObject, IStartupWindow
  {
    private Shell FShell;
    private OakApplication FAppModel;

    public OakApplication App { get { return FAppModel; } }

    public ReactiveCommand NewLogCommand { get; set; }
    public ReactiveCommand NewKeepAliveCommand { get; set; }

    public StartupViewModel(OakApplication aApp, Shell aShell)
    {
      FShell = aShell;
      FAppModel = aApp;

      Title = "App settings";

      FAppModel.ObservableForProperty(x => x.ServerUrl)
        .Subscribe(v=>
        {
          Properties.Settings.Default.ServerUrl = v.Value;
          Properties.Settings.Default.Save();
        });

      var _canNew = FAppModel.WhenAny(x => x.ServerUrl, x => !String.IsNullOrWhiteSpace(x.Value));
      NewLogCommand = ReactiveCommand.Create(NewLog, _canNew);
      NewKeepAliveCommand = ReactiveCommand.Create(NewLog, _canNew);
    }

    [Reactive]
    public bool CanClose { get; set; } = true;
    [Reactive]
    public ReactiveCommand CloseCommand { get; set; } = null;
    [Reactive]
    public string Title { get; set; }

    private void NewLog()
    {
      var _log = Bootstrap.Container.Resolve<ILogsWindow>();
      var _props = Bootstrap.Container.Resolve<IPropertiesWindow>();
      var _sources = Bootstrap.Container.Resolve<ISourcesWindow>();

      FShell.ShowDocument(_log);
      FShell.ShowTool(_props);
      FShell.ShowTool(_sources);

      FShell.SelectedWindow = _log;
    }
  }
}

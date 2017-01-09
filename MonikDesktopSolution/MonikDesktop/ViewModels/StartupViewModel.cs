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
    private MApp FApp;

    public MApp App { get { return FApp; } }

    public ReactiveCommand NewLogCommand { get; set; }
    public ReactiveCommand NewKeepAliveCommand { get; set; }

    public StartupViewModel(MApp aApp, Shell aShell)
    {
      FShell = aShell;
      FApp = aApp;

      Title = "App settings";

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

      FShell.Show(_log);
      FShell.Show(_props);
      FShell.Show(_sources);

      FApp.SelectedWindow = _log;
    }
  }
}

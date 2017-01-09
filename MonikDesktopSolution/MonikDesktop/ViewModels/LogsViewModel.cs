using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

namespace MonikDesktop.ViewModels
{
  public class LogsViewModel : ReactiveObject, ILogsWindow
  {
    private MApp FApp;
    private IMonikService FService;
    private ISourcesCache FCache;

    private LogsModel FModel;
    public ShowModel Model { get { return FModel; } }
    
    // TODO: alert if receivedtime < createdtime or receivedtime >> createdtime
    public ReactiveList<LogItem> LogsList { get; set; }

    public ReactiveCommand StartCommand { get; set; }
    public ReactiveCommand StopCommand { get; set; }
    public ReactiveCommand<Unit, LogItem[]> UpdateCommand { get; set; }

    public LogsViewModel(MApp aApp, IMonikService aService, ISourcesCache aCache)
    {
      FApp = aApp;
      FService = aService;
      FCache = aCache;

      LogsList = new ReactiveList<LogItem>();
      FModel = new LogsModel();

      FModel.Caption = "Logs";

      // TODO:
      //FModel.WhenAnyValue(x => x.Caption, x => x.Online)
        //.Subscribe(v => this.DisplayName = v.Item1 + (v.Item2 ? " >" : " ||"));

      var _canStart = FModel.WhenAny(x => x.Online, x => !x.Value);
      StartCommand = ReactiveCommand.Create(OnStart, _canStart);
      
      var _canStop = FModel.WhenAny(x => x.Online, x => x.Value);
      StopCommand = ReactiveCommand.Create(OnStop, _canStop);

      UpdateCommand = ReactiveCommand.Create<LogItem[]>(OnUpdate, _canStop);
      UpdateCommand.Subscribe(results =>
      {
        foreach (var it in results)
          LogsList.Add(it);
      });

      UpdateCommand.ThrownExceptions
        .Subscribe(ex =>
        {
          // TODO: handle
        });

      /*FModel
        .WhenAnyValue(x => x.Online)
        .Where(v => v)
        .Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
        .Select(_ => Unit.Default)
        .InvokeCommand(StartCommand);*/
    }

    private IDisposable FUpdateExecutor = null;

    private void OnStart()
    {
      LastID = null;
      LogsList.Clear();

      var interval = TimeSpan.FromSeconds(FModel.RefreshSec);
      FUpdateExecutor = Observable
          .Timer(interval, interval)
          .Select(_ => Unit.Default)
          .InvokeCommand(UpdateCommand);

      Model.Online = true;
    }

    private void OnStop()
    {
      if (FUpdateExecutor == null)
        return;

      FUpdateExecutor.Dispose();
      FUpdateExecutor = null;

      FModel.Online = false;
    }

    private LogItem[] OnUpdate()
    {
      var _req = new ELogRequest()
      {
        LastID = this.LastID,
        Level = FModel.Level == LevelType.None ? null : (byte?)FModel.Level,
        SeverityCutoff = FModel.SeverityCutoff == SeverityCutoffType.None ? null : (byte?)FModel.SeverityCutoff,
        Top = FModel.Top == TopType.None ? null : (byte?)FModel.Top,
        Groups = FModel.Groups.ToArray(),
        Instances = FModel.Instances.ToArray()
      };
      var _response = FService.GetLogs(_req);

      if (_response.Length > 0)
        LastID = _response.Max(x => x.ID);

      var _res = _response.Select(x =>
              new LogItem()
              {
                ID = x.ID,
                Created = x.Created.ToLocalTime(),
                CreatedStr = x.Created.ToString(Model.DateTimeFormat),
                Received = x.Received.ToLocalTime(),
                Level = x.Level,
                Severity = x.Severity,
                Instance = FCache.GetInstance(x.InstanceID),
                Body = x.Body
              });

      return _res.ToArray();
    }

    public long? LastID { get; private set; } = null;

    [Reactive]
    public string Title { get; set; }
    [Reactive]
    public bool CanClose { get; set; } = true;
    [Reactive]
    public ReactiveCommand CloseCommand { get; set; } = null;
  }
}

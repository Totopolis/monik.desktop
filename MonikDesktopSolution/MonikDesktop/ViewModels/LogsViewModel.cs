using Autofac;
using MonikDesktop.Common.Enums;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApi;
using MonikDesktop.Common.ModelsApp;
using MonikDesktop.ViewModels.ShowModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class LogsViewModel : ViewModelBase, ILogsViewModel
	{
		private readonly LogsModel _model;
		
		private IDisposable _updateExecutor;

		public LogsViewModel(IShell aShell, ISourcesCacheProvider cacheProvider)
		{
			LogsList = new ReactiveList<LogItem>();
		    _model = new LogsModel
		    {
		        Caption = "Logs",
		        Cache = cacheProvider.CurrentCache
		    };
            Disposables.Add(_model);

			_model.WhenAnyValue(x => x.Caption, x => x.Online)
				.Subscribe(v => Title = v.Item1 + (v.Item2 ? " >" : " ||"));

			var canStart = _model.WhenAny(x => x.Online, x => !x.Value);
			StartCommand = ReactiveCommand.Create(OnStart, canStart);

			var canStop = _model.WhenAny(x => x.Online, x => x.Value);
			StopCommand = ReactiveCommand.Create(OnStop, canStop);

			UpdateCommand = ReactiveCommand.Create(OnUpdate, canStop);
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

			_model.ObservableForProperty(x => x.SelectedItem)
				.Subscribe(v =>
				{
					var desc = aShell.Container.Resolve<ILogDescriptionViewModel>();
					desc.SelectedItem = v.Value;
				});

			/*_model
			  .WhenAnyValue(x => x.Online)
			  .Where(v => v)
			  .Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
			  .Select(_ => Unit.Default)
			  .InvokeCommand(StartCommand);*/
		}

		// TODO: alert if receivedtime < createdtime or receivedtime >> createdtime
		public ReactiveList<LogItem> LogsList { get; set; }
		public ReactiveCommand<Unit, LogItem[]> UpdateCommand { get; set; }

		public long? LastId { get; private set; }

		public ShowModel Model => _model;

		public ReactiveCommand StartCommand { get; set; }
		public ReactiveCommand StopCommand { get; set; }

        private void OnStart()
		{
			LastId = null;
			LogsList.Clear();

			var interval = TimeSpan.FromSeconds(_model.RefreshSec);
			_updateExecutor = Observable
				.Timer(interval, interval)
				.Select(_ => Unit.Default)
				.InvokeCommand(UpdateCommand);

		    _model.Online = true;
		}

		private void OnStop()
		{
			if (_updateExecutor == null)
				return;

			_updateExecutor.Dispose();
			_updateExecutor = null;

			_model.Online = false;
		}

		private LogItem[] OnUpdate()
		{
			var req = new ELogRequest
			{
				LastID         = LastId,
				Level          = _model.Level == LevelType.None ? null : (byte?) _model.Level,
				SeverityCutoff = _model.SeverityCutoff == SeverityCutoffType.None ? null : (byte?) _model.SeverityCutoff,
				Top            = _model.Top == TopType.None ? null : (byte?) _model.Top,
				Groups         = _model.Groups.ToArray(),
				Instances      = _model.Instances.ToArray()
			};

			ELog_[] response;

			try
			{
				response = _model.Cache.Service.GetLogs(req);
			}
			catch
			{
			    return new[] {new LogItem {Body = "INTERNAL ERROR", Created = DateTime.Now}};
			}

			if (response.Length > 0)
				LastId = response.Max(x => x.ID);

		    IEnumerable<ELog_> groupedRequest;
		    if (_model.GroupDuplicatingItems)
		        groupedRequest = GroupDuplicatingLogs(response).OrderBy(x => x.ID);
		    else
		        groupedRequest = response;

		    var result = groupedRequest.Select(x =>
			{
			    var created = x.Created.ToLocalTime();
			    return new LogItem
			    {
			        ID          = x.ID,
			        Created     = created,
			        CreatedStr  = created.ToString(x.Doubled? _model.DuplicatedDateTimeFormat : _model.DateTimeFormat),
			        Received    = x.Received.ToLocalTime(),
			        ReceivedStr = x.Received.ToLocalTime().ToString(_model.DateTimeFormat),
			        Level       = x.Level,
			        Severity    = x.Severity,
			        Instance    = _model.Cache.GetInstance(x.InstanceID),
			        Body        = x.Body.Replace(Environment.NewLine, "")
			    };
			});

			return result.ToArray();
		}
        
	    private IEnumerable<ELog_> GroupDuplicatingLogs(ELog_[] response)
	    {
	        var result = response?.GroupBy(r => new { r.InstanceID, r.Body, r.Severity, r.Level }).SelectMany(g =>
	        {
	            var min = g.Min(r => r.Created);
	            var firstIn5Sec = g.GroupBy(r =>
	                {
	                    var totalSeconds = (r.Created - min).TotalSeconds;
	                    var rez = totalSeconds - totalSeconds % _model.RefreshSec;

	                    return rez;
	                })
	                .Select(inner =>
	                {
	                    var eLog = inner.First();
	                    if (inner.Count() > 1)
	                        eLog.Doubled = true;
	                    return eLog;
	                }).ToArray();

	            return firstIn5Sec;
	        });
	        return result;
	    }
    }
}
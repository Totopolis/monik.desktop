using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Doaking.Core.Oak;
using MonikDesktop.Common.Enums;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApi;
using MonikDesktop.Common.ModelsApp;
using MonikDesktop.ViewModels.ShowModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop.ViewModels
{
	public class LogsViewModel : ReactiveObject, ILogsWindow
	{
		private readonly IMonikService _service;
		private readonly ISourcesCache _cache;

		private readonly LogsModel _model;
		 
		private IDisposable _updateExecutor;

		public LogsViewModel(Shell aShell, IMonikService aService, ISourcesCache aCache)
		{
			_service = aService;
			_cache = aCache;

			LogsList = new ReactiveList<LogItem>();
			_model = new LogsModel {Caption = "Logs"};

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
					var desc = aShell.Resolve<ILogDescription>();
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

		[Reactive]
		public string Title { get; set; }

		[Reactive]
		public bool CanClose { get; set; } = true;

		[Reactive]
		public ReactiveCommand CloseCommand { get; set; } = null;

		private void OnStart()
		{
			LastId = null;
			LogsList.Clear();

			var interval = TimeSpan.FromSeconds(_model.RefreshSec);
			_updateExecutor = Observable
				.Timer(interval, interval)
				.Select(_ => Unit.Default)
				.InvokeCommand(UpdateCommand);

			Model.Online = true;
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
				LastID = LastId,
				Level = _model.Level == LevelType.None ? null : (byte?) _model.Level,
				SeverityCutoff = _model.SeverityCutoff == SeverityCutoffType.None ? null : (byte?) _model.SeverityCutoff,
				Top = _model.Top == TopType.None ? null : (byte?) _model.Top,
				Groups = _model.Groups.ToArray(),
				Instances = _model.Instances.ToArray()
			};

			ELog_[] response;

			try
			{
				response = _service.GetLogs(req);
			}
			catch
			{
				return new[] {new LogItem {Body = "INTERNAL ERROR"}};
			}

			if (response.Length > 0)
				LastId = response.Max(x => x.ID);

			var result = response.Select(x =>
				new LogItem
				{
					ID = x.ID,
					Created = x.Created.ToLocalTime(),
					CreatedStr = x.Created.ToLocalTime().ToString(Model.DateTimeFormat),
					Received = x.Received.ToLocalTime(),
					ReceivedStr = x.Received.ToLocalTime().ToString(Model.DateTimeFormat),
					Level = x.Level,
					Severity = x.Severity,
					Instance = _cache.GetInstance(x.InstanceID),
					Body = x.Body.Replace(Environment.NewLine, "")
				});

			return result.ToArray();
		}
	}
}
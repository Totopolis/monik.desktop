﻿using DynamicData;
using MonikDesktop.Common.Enums;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApi;
using MonikDesktop.Common.ModelsApp;
using MonikDesktop.ViewModels.ShowModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class LogsViewModel : ViewModelBase, IShowViewModel
    {
        private readonly LogsModel _model;

        private IDisposable _updateExecutor;

        public LogsViewModel(IShell aShell, ISourcesCacheProvider cacheProvider)
        {
            ScrollTo = new Interaction<LogItem, Unit>();

            LogsSource = new SourceList<LogItem>();

            LogsSource
                .Connect()
                .ObserveOnDispatcher()
                .Bind(out _logsList)
                .Subscribe()
                .DisposeWith(Disposables);

            _model = new LogsModel
            {
                Caption = "Logs",
                Cache = cacheProvider.CurrentCache
            }.DisposeWith(Disposables);

            _model.WhenAnyValue(x => x.Caption, x => x.Online)
                .ObserveOnDispatcher()
                .Subscribe(v => Title = v.Item1 + (v.Item2 ? " >" : " ||"))
                .DisposeWith(Disposables);

            _model.ObservableForProperty(x => x.Online)
                .Subscribe(p =>
                {
                    if (p.Value)
                    {
                        LastId = null;
                        LogsSource.Clear();

                        var interval = TimeSpan.FromSeconds(_model.RefreshSec);
                        _updateExecutor = Observable
                            .Timer(interval, interval)
                            .Select(_ => Unit.Default)
                            .InvokeCommand(UpdateCommand);
                    }
                    else
                        _updateExecutor?.Dispose();
                })
                .DisposeWith(Disposables);

            var canStart = _model.WhenAny(x => x.Online, x => !x.Value);
            StartCommand = ReactiveCommand.Create(() =>
            {
                _model.Online = true;
                return Unit.Default;
            }, canStart);

            var canStop = _model.WhenAny(x => x.Online, x => x.Value);
            StopCommand = ReactiveCommand.Create(() =>
            {
                _model.Online = false;
                return Unit.Default;
            }, canStop);

            UpdateCommand = ReactiveCommand.Create(OnUpdate);
            UpdateCommand.Subscribe(result =>
            {
                LogsSource.AddRange(result);
            });

            Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    h => ((INotifyCollectionChanged) _logsList).CollectionChanged += h,
                    h => ((INotifyCollectionChanged) _logsList).CollectionChanged -= h)
                .Where(x => x.EventArgs.Action == NotifyCollectionChangedAction.Add)
                .Throttle(TimeSpan.FromSeconds(.1), RxApp.MainThreadScheduler)
                .Where(_ => _model.AutoScroll)
                .Subscribe(_ =>
                {
                    var last = LogsSource.Items.LastOrDefault();
                    if (last != null)
                        ScrollTo.Handle(last).Wait();
                })
                .DisposeWith(Disposables);
        }

        public Interaction<LogItem, Unit> ScrollTo { get; set; }
        public SourceList<LogItem> LogsSource { get; set; }
        private ReadOnlyObservableCollection<LogItem> _logsList;
        public ReadOnlyObservableCollection<LogItem> LogsList => _logsList;
        public ReactiveCommand<Unit, LogItem[]> UpdateCommand { get; set; }

        public long? LastId { get; private set; }

        public ShowModel Model => _model;

        public ReactiveCommand<Unit, Unit> StartCommand { get; set; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; set; }

        private LogItem[] OnUpdate()
        {
            var req = new ELogRequest
            {
                LastID = LastId,
                Level = _model.Level == LevelType.None ? null : (byte?) _model.Level,
                SeverityCutoff =
                    _model.SeverityCutoff == SeverityCutoffType.None ? null : (byte?) _model.SeverityCutoff,
                Top = _model.Top == TopType.None ? null : (byte?) _model.Top,
                Groups = _model.Groups.ToArray(),
                Instances = _model.Instances.ToArray()
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
                    ID = x.ID,
                    Created = created,
                    CreatedStr = created.ToString(x.Doubled ? _model.DuplicatedDateTimeFormat : _model.DateTimeFormat),
                    Received = x.Received.ToLocalTime(),
                    ReceivedStr = x.Received.ToLocalTime().ToString(_model.DateTimeFormat),
                    Level = x.Level,
                    Severity = x.Severity,
                    Instance = _model.Cache.GetInstance(x.InstanceID),
                    Body = x.Body.Replace(Environment.NewLine, "")
                };
            });

            return result.ToArray();
        }

        private IEnumerable<ELog_> GroupDuplicatingLogs(ELog_[] response)
        {
            var result = response?.GroupBy(r => new {r.InstanceID, r.Body, r.Severity, r.Level}).SelectMany(g =>
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

        protected override void DisposeInternals()
        {
            base.DisposeInternals();
            _updateExecutor?.Dispose();
        }
    }
}
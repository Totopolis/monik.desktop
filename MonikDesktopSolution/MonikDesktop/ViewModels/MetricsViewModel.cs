using LiveCharts;
using LiveCharts.Configurations;
using MonikDesktop.Common;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;
using MonikDesktop.ViewModels.ShowModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class MetricsViewModel : ViewModelBase, IShowViewModel
    {
        private readonly MetricsModel _model;

        private IDisposable _updateExecutor;

        public MetricsViewModel(ISourcesCacheProvider cacheProvider)
        {
            _model = new MetricsModel
            {
                Caption = "Metrics",
                Cache = cacheProvider.CurrentCache
            };
            Disposables.Add(_model);

            _model.WhenAnyValue(x => x.Caption, x => x.Online)
               .Subscribe(v => Title = v.Item1 + (v.Item2 ? " >" : " ||"));

            var dynamicFilter = _model.SelectedSourcesChanged
                .Select(_ => Filters.CreateFilterMetricBySources(_model));

            var observable = _model.Cache.Metrics
                .Connect()
                .Filter(dynamicFilter)
                .Publish();

            _metricsCache = observable
                .AsObservableCache()
                .DisposeWith(Disposables);

            observable
                .Transform(x => new MetricValueItem() {Metric = x})
                .Bind(out _metricValuesList)
                .Subscribe()
                .DisposeWith(Disposables);

            observable
                .Connect()
                .DisposeWith(Disposables);

            _model.SelectedSourcesChanged.Subscribe(_ => UpdateSelectedMetrics());
            UpdateSelectedMetrics();
            
            var canStart = _model.WhenAny(x => x.Online, x => !x.Value);
            StartCommand = ReactiveCommand.Create(OnStart, canStart);

            var canStop = _model.WhenAny(x => x.Online, x => x.Value);
            StopCommand = ReactiveCommand.Create(OnStop, canStop);

            UpdateCommand = ReactiveCommand.Create(OnUpdate, canStop);


            var mapper = Mappers.Xy<MetricValueItem>()
                .X(item => (double) item.Interval.Ticks / TimeSpan.FromMinutes(5).Ticks)
                .Y(item => item.Value);

            //lets save the mapper globally.
            Charting.For<MetricValueItem>(mapper);

            SeriesCollection = new ChartValues<MetricValueItem>();

            //lets set how to display the X Labels
            DateTimeFormatter = value => new DateTime((long)value * TimeSpan.FromMinutes(5).Ticks).ToString("t");
            
            UpdateCommand.Subscribe(results =>
            {
                if (_model.MetricDiagramVisible)
                {
                    SeriesCollection.Clear();
                    SeriesCollection.AddRange(results);
                }
                // ToDo
                //else
                //{
                //    MetricValuesList.Initialize(results);
                //}
            });
        }

        private readonly IObservableCache<Metric, int> _metricsCache;
        private readonly ReadOnlyObservableCollection<MetricValueItem> _metricValuesList;
        public ReadOnlyObservableCollection<MetricValueItem> MetricValuesList => _metricValuesList;
        public ReactiveCommand<Unit, IEnumerable<MetricValueItem>> UpdateCommand { get; set; }
        public ReactiveCommand StartCommand { get; set; }
        public ReactiveCommand StopCommand { get; set; }

        public ChartValues<MetricValueItem> SeriesCollection { get; set; }

        public ShowModel Model => _model;

        [Reactive] public MetricValueItem SelectedMetric { get; set; }

        public Func<double, string> DateTimeFormatter { get; set; }

        private void UpdateSelectedMetrics()
        {
            if (SelectedMetric == null)
                SelectedMetric = MetricValuesList.FirstOrDefault();
        }

        private void OnStart()
        {
            if (_model.MetricDiagramVisible)
                SeriesCollection.Clear();

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

        private IEnumerable<MetricValueItem> OnUpdate()
        {
            IEnumerable<MetricValueItem> response = null;

            try
            {
                switch (_model.MetricTerminalMode)
                {
                    case MetricTerminalMode.Current:

                        response = _model.Cache.Service.GetCurrentMetricValues()
                           .Select(x => new {m = _metricsCache.Lookup(x.MetricId), v = x})
                           .Where(x => x.m.HasValue)
                           .Select(x => new MetricValueItem
                           {
                               Metric = x.m.Value,
                               Value = Math.Round(x.v.Value, 2),
                               HasValue = true,
                               Interval = x.v.Interval.ToLocalTime()
                           });

                        break;

                    case MetricTerminalMode.TimeWindow:

                        response = _model.Cache.Service.GetWindowMetricValues()
                            .Select(x => new { m = _metricsCache.Lookup(x.MetricId), v = x })
                            .Where(x => x.m.HasValue)
                            .Select(x => new MetricValueItem
                            {
                                Metric = x.m.Value,
                                Value = Math.Round(x.v.Value, 2),
                                HasValue = true
                            });

                        break;

                    case MetricTerminalMode.Diagram:

                        if (SelectedMetric == null) break;
                        var amount = _model.MetricHistoryDepthHours * 12;
                        var history = _model.Cache.Service.GetMetricHistory(SelectedMetric.Metric.ID, amount, _model.MetricHistorySkip5Min);

                        response = history.Values
                            .Select((v, i) => new MetricValueItem()
                            {
                                Interval = history.Interval.AddMinutes(-5 * i).ToLocalTime(),
                                Metric = SelectedMetric.Metric,
                                Value = v,
                                HasValue = true
                            });

                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception)
            {
                return new[]
                {
                    new MetricValueItem
                    {
                        Metric = new Metric
                        {
                            ID   = -1,
                            Name = "",
                            Instance = new Instance
                            {
                                ID     = -1,
                                Name   = "INTERNAL",
                                Source = new Source {ID = -1, Name = "ERROR"}
                            },
                        }
                    }
                };
            }

            response = response
                .OrderBy(x => x.Metric.Instance.Source.Name)
                .ThenBy(x => x.Metric.Instance.Name)
                .ThenBy(x => x.Metric.Name)
                .ToList();

            return response;
        }
    }
}

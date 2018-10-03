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
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class MetricsViewModel : ViewModelBase, IMetricsViewModel
    {
        private readonly MetricsModel _model;

        private Dictionary<int, Metric> _metrics = new Dictionary<int, Metric>();

        private IDisposable _updateExecutor;

        public MetricsViewModel(ISourcesCacheProvider cacheProvider)
        {
            MetricValuesList = new ReactiveList<MetricValueItem>();
            _model = new MetricsModel
            {
                Caption = "Metrics",
                Cache = cacheProvider.CurrentCache
            };

            _model.WhenAnyValue(x => x.Caption, x => x.Online)
               .Subscribe(v => Title = v.Item1 + (v.Item2 ? " >" : " ||"));

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
                else
                {
                    MetricValuesList.Initialize(results);
                }
            });

            UpdateCommand.ThrownExceptions
               .Subscribe(ex =>
                {
                    // TODO: handle
                });
        }

        // TODO: alert if receivedtime < createdtime or receivedtime >> createdtime
        public ReactiveList<MetricValueItem> MetricValuesList { get; set; }
        public ReactiveCommand<Unit, IEnumerable<MetricValueItem>> UpdateCommand { get; set; }
        public ReactiveCommand StartCommand { get; set; }
        public ReactiveCommand StopCommand { get; set; }

        public ChartValues<MetricValueItem> SeriesCollection { get; set; }

        public ShowModel Model => _model;

        [Reactive] public MetricValueItem SelectedMetric { get; set; }

        public Func<double, string> DateTimeFormatter { get; set; }

        private void UpdateSelectedMetrics()
        {
            _metrics = GetMetrics()
                .Where(IsNeedToShowMetricDescription)
                .ToDictionary(md => md.ID);

            var data = _metrics.Values
                .Select(metric => new MetricValueItem() {Metric = metric});
            MetricValuesList.Initialize(data);
            
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

        private List<Metric> GetMetrics()
        {
            return _model.Cache.Metrics
               .OrderBy(md => md.Instance.Source.Name)
               .ThenBy(md => md.Instance.Name)
               .ThenBy(md => md.Name)
               .ToList();
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
                           .Where(x => _metrics.ContainsKey(x.MetricId))
                           .Select(x => new MetricValueItem
                           {
                               Metric = _metrics[x.MetricId],
                               Value = Math.Round(x.Value, 2),
                               HasValue = true,
                               Interval = x.Interval.ToLocalTime()
                           });

                        break;

                    case MetricTerminalMode.TimeWindow:

                        response = _model.Cache.Service.GetWindowMetricValues()
                            .Where(x => _metrics.ContainsKey(x.MetricId))
                            .Select(x => new MetricValueItem
                            {
                                Metric = _metrics[x.MetricId],
                                Value = Math.Round(x.Value, 2),
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
            catch (Exception ex)
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

        private bool IsNeedToShowMetricDescription(Metric md)
        {
            if (!_model.Instances.Any() && !_model.Groups.Any())
                return true;

            if (_model.Instances.Contains(md.Instance.ID))
                return true;

            var groupId = _model.Cache.Groups.FirstOrDefault(x => x.Instances.Contains(md.Instance))?.ID;
            return groupId.HasValue && _model.Groups.Contains(groupId.Value);
        }
    }
}

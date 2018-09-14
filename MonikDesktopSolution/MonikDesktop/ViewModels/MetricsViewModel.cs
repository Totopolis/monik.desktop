using LiveCharts;
using LiveCharts.Configurations;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApi;
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
        private readonly IMonikService _service;
        private readonly ISourcesCache _cache;

        private readonly MetricsModel _model;

        private Dictionary<int, MetricDescription> _metricDescriptions = new Dictionary<int, MetricDescription>();

        private IDisposable _updateExecutor;
        private DateTime _timeWindowBegin;
        private DateTime _timeWindowPop;
        private int _windowPop;

        public MetricsViewModel(IMonikService aService, ISourcesCache aCache)
        {
            _service = aService;
            _cache = aCache;

            MetricValuesList = new ReactiveList<MetricValueItem>();
            _model = new MetricsModel { Caption = "Metrics" };

            _model.WhenAnyValue(x => x.Caption, x => x.Online)
               .Subscribe(v => Title = v.Item1 + (v.Item2 ? " >" : " ||"));

            var canStart = _model.WhenAny(x => x.Online, x => !x.Value);
            StartCommand = ReactiveCommand.Create(OnStart, canStart);

            var canStop = _model.WhenAny(x => x.Online, x => x.Value);
            StopCommand = ReactiveCommand.Create(OnStop, canStop);

            UpdateCommand = ReactiveCommand.Create(OnUpdate, canStop);


            var mapper = Mappers.Xy<MetricValueItem>()
               .X(item => item.Interval.Ticks)
               .Y(item => item.Value);

            //lets save the mapper globally.
            Charting.For<MetricValueItem>(mapper);

            SeriesCollection = new ChartValues<MetricValueItem>();

            //lets set how to display the X Labels
            DateTimeFormatter = value => new DateTime((long)value).ToString("t");

            AxisUnit = TimeSpan.TicksPerSecond;
            AxisStep = _model.MetricSecInterval * _model.WindowIntervalWidth * TimeSpan.TicksPerSecond;

            AxisMax = DateTime.Now.AddMinutes(2).Ticks;
            AxisMin = DateTime.Now.Ticks - AxisStep;

            //SelectedMetric = null;

            UpdateCommand.Subscribe(results =>
            {
                if (_model.MetricTerminalMode != MetricTerminalMode.Diagramm)
                {
                    MetricValuesList.Clear();

                    //workaround since ReactiveList.AddRange(results); throws UnsupportedException for collections with 2-10 items
                    //https://github.com/reactiveui/ReactiveUI/issues/1354
                    foreach (var item in results)
                        MetricValuesList.Add(item);
                }
                else
                {
                    //SeriesCollection.Clear();
                    SeriesCollection.Add(results.Last());

                    AxisMax = results.Last().Interval.Ticks / TimeSpan.TicksPerSecond;
                    AxisMin = results.First().Interval.Ticks / TimeSpan.TicksPerSecond;

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
        [Reactive] public long AxisUnit { get; set; }
        [Reactive] public long AxisStep { get; set; }

        //TODO Find why changes of these properties are not updated at View
        [Reactive] public long AxisMin { get; set; }
        [Reactive] public long AxisMax { get; set; }

        public Func<double, string> DateTimeFormatter { get; set; }


        private void OnStart()
        {
            MetricValuesList.Clear();
            _metricDescriptions = GetMetricDescriptions().ToDictionary(md => md.Id);

            if (_model.MetricTerminalMode == MetricTerminalMode.Diagramm)
            {
                foreach (var md in _metricDescriptions.Values.Select(d => new MetricValueItem() { Description = d }))
                    MetricValuesList.Add(md);

                SelectedMetric = MetricValuesList.FirstOrDefault();


                SeriesCollection.Clear();

                //AxisStep forces the distance between each separator in the X axis
                AxisStep = _model.MetricSecInterval * _model.WindowIntervalWidth * TimeSpan.TicksPerSecond;

                AxisMax = DateTime.Now.AddMinutes(2).Ticks;
                AxisMin = DateTime.Now.Ticks - AxisStep;


                _timeWindowBegin = DateTime.UtcNow.Date +
                                   TimeSpan.FromSeconds((int)DateTime.UtcNow.TimeOfDay.TotalSeconds /
                                                        (_model.WindowIntervalWidth * _model.MetricSecInterval) *
                                                        (_model.WindowIntervalWidth * _model.MetricSecInterval));

                _windowPop = (int)(DateTime.UtcNow - _timeWindowBegin).TotalSeconds / _model.MetricSecInterval;
                _timeWindowPop = _timeWindowBegin + TimeSpan.FromSeconds(_windowPop * _model.MetricSecInterval);
            }

            var interval = TimeSpan.FromSeconds(_model.RefreshSec);

            _updateExecutor = Observable
               .Timer(interval, interval)
               .Select(_ => Unit.Default)
               .InvokeCommand(UpdateCommand);

            Model.Online = true;
        }

        private List<MetricDescription> GetMetricDescriptions()
        {
            IEnumerable<EMetricDescription> eMetricDescriptions;

            try
            {
                eMetricDescriptions = _service.GetMetricDescriptions();
            }
            catch
            {
                return new List<MetricDescription>
                {
                    new MetricDescription
                    {
                        Id   = -1,
                        Name = "",
                        Instance = new Instance
                        {
                            ID     = -1,
                            Name   = "INTERNAL",
                            Source = new Source {ID = -1, Name = "ERROR"}
                        },
                    }
                };
            }

            return eMetricDescriptions
               .Select(eMd => new MetricDescription()
               {
                   Id = (int)eMd.Id,
                   Name = eMd.Name,
                   Instance = _cache.GetInstance(eMd.InstanceId),
                   Type = (MetricType)eMd.Aggregation,
               })
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

                        response = _service.GetCurrentMetricValues()
                           .Select(x => new MetricValueItem
                           {
                               Description = _metricDescriptions[x.MetricId],
                               Value = Math.Round(x.Value, 2),
                               Interval = x.Interval.ToLocalTime()
                           });

                        break;

                    case MetricTerminalMode.TimeWindow:

                        response = _service.GetWindowMetricValues()
                            .Select(x => new MetricValueItem
                            {
                                Description = _metricDescriptions[x.MetricId],
                                Value = Math.Round(x.Value, 2)
                            });

                        break;

                    case MetricTerminalMode.Diagramm:

                        if (SelectedMetric == null) break;

                        if (_timeWindowPop.AddSeconds(_model.MetricSecInterval) <= DateTime.UtcNow)
                        {
                            _timeWindowPop = _timeWindowPop.AddSeconds(_model.MetricSecInterval);
                            _windowPop++;

                            if (_windowPop >= _model.WindowIntervalWidth)
                            {
                                _timeWindowBegin = _timeWindowPop;
                                _windowPop = 0;
                            }
                        }

                        var history = _service.GetHistoryMetricValues(new EHistoryMetricsRequest()
                        {
                            MetricId = SelectedMetric.Description.Id,
                            Count = _model.WindowIntervalWidth * _model.MetricAggWindowsDepth + _windowPop
                        });

                        history.Reverse();

                        var current = _service.GetCurrentMetricValue(SelectedMetric.Description.Id);

                        var currMetricItem = new MetricValueItem()
                        {
                            Interval = current.Interval.ToLocalTime(),
                            Description = SelectedMetric.Description,
                            Value = current.Value + history.Take(_windowPop).Sum(h => h.Value),
                            AggValuesCount = _windowPop + 1
                        };

                        var historyMetricItems = history.Skip(_windowPop)
                           .Select((h, index) => new { h, index })
                           .GroupBy(x => x.index / _model.WindowIntervalWidth)
                           .Select(g => new MetricValueItem()
                           {
                               Interval = g.First().h.Interval.ToLocalTime(),
                               Description = SelectedMetric.Description,
                               Value = g.Sum(m => m.h.Value),
                               AggValuesCount = g.Count()
                           });

                        response = new List<MetricValueItem>() { currMetricItem };
                        ((List<MetricValueItem>)response).AddRange(historyMetricItems);

                        foreach (var item in response)
                            item.AggValue();

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
                        Description = new MetricDescription
                        {
                            Id   = -1,
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

            response = response.OrderBy(x => x.Description.Instance.Source.Name)
               .ThenBy(x => x.Description.Instance.Name)
               .ThenBy(x => x.Description.Name)
               .ToList();

            return response;
        }
    }
}

using LiveCharts;
using LiveCharts.Configurations;
using MonikDesktop.Common;
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

        public MetricsViewModel(IMonikService aService, ISourcesCache aCache)
        {
            _service = aService;
            _cache = aCache;

            MetricValuesList = new ReactiveList<MetricValueItem>();
            _model = new MetricsModel { Caption = "Metrics" };

            _model.WhenAnyValue(x => x.Caption, x => x.Online)
               .Subscribe(v => Title = v.Item1 + (v.Item2 ? " >" : " ||"));

            _model.WhenAnyValue(x => x.MetricHistoryDepthHours)
                .Subscribe(v => SeriesColumnPadding = v > 5 ? 0 : v > 2 ? 1 : 2);

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
                if (_model.MetricTerminalMode == MetricTerminalMode.Diagram)
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
        [Reactive] public int SeriesColumnPadding { get; set; }

        public Func<double, string> DateTimeFormatter { get; set; }


        private void OnStart()
        {
            MetricValuesList.Clear();
            _metricDescriptions = GetMetricDescriptions()
                .Where(IsNeedToShowMetricDescription)
                .ToDictionary(md => md.Id);

            if (_model.MetricTerminalMode == MetricTerminalMode.Diagram)
            {
                var data = _metricDescriptions.Values
                    .Select(d => new MetricValueItem() {Description = d});
                MetricValuesList.AddRange(data);

                SelectedMetric = MetricValuesList.FirstOrDefault();


                SeriesCollection.Clear();
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
                           .Where(x => _metricDescriptions.ContainsKey(x.MetricId))
                           .Select(x => new MetricValueItem
                           {
                               Description = _metricDescriptions[x.MetricId],
                               Value = Math.Round(x.Value, 2),
                               Interval = x.Interval.ToLocalTime()
                           });

                        break;

                    case MetricTerminalMode.TimeWindow:

                        response = _service.GetWindowMetricValues()
                            .Where(x => _metricDescriptions.ContainsKey(x.MetricId))
                            .Select(x => new MetricValueItem
                            {
                                Description = _metricDescriptions[x.MetricId],
                                Value = Math.Round(x.Value, 2)
                            });

                        break;

                    case MetricTerminalMode.Diagram:

                        if (SelectedMetric == null) break;
                        var amount = _model.MetricHistoryDepthHours * 12;
                        var history = _service.GetMetricHistory(SelectedMetric.Description.Id, amount, _model.MetricHistorySkip5Min);

                        response = history.Values
                            .Select((v, i) => new MetricValueItem()
                            {
                                Interval = history.Interval.AddMinutes(-5 * i).ToLocalTime(),
                                Description = SelectedMetric.Description,
                                Value = v,
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

            response = response
                .OrderBy(x => x.Description.Instance.Source.Name)
                .ThenBy(x => x.Description.Instance.Name)
                .ThenBy(x => x.Description.Name)
                .ToList();

            return response;
        }

        private bool IsNeedToShowMetricDescription(MetricDescription md)
        {
            if (_model.Instances.IsEmpty && _model.Groups.IsEmpty)
                return true;

            if (_model.Instances.Contains(md.Instance.ID))
                return true;

            var groupId = _cache.Groups.FirstOrDefault(x => x.Instances.Contains(md.Instance))?.ID;
            return groupId.HasValue && _model.Groups.Contains(groupId.Value);
        }
    }
}

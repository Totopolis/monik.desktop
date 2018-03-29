using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using MonikDesktop.Common.ModelsApi;
using MonikDesktop.Common.ModelsApp;
using MonikDesktop.ViewModels.ShowModels;

namespace SampleDatas
{
    public class MetricSamples
    {
        public List<MetricValueItem> MetricValuesList =>
            new List<MetricValueItem>()
            {
                new MetricValueItem()
                {
                    Description = new MetricDescription()
                    {
                        Id = 1,
                        Instance = new Instance()
                        {
                            ID = 1,
                            Source = new Source()
                                {ID = 1, Name = "testSource"},
                            Name = "testInstance"
                        },
                        Name = "testMetricGau",
                        Type = MetricType.Gauge
                    },
                    Value = 100
                },
                new MetricValueItem()
                {
                    Description = new MetricDescription()
                    {
                        Id = 2,
                        Instance = new Instance()
                        {
                            ID = 1,
                            Source = new Source()
                                {ID = 1, Name = "testSource"},
                            Name = "testInstance"
                        },
                        Name = "testMetricAcc",
                        Type = MetricType.Accumulator
                    },
                    Value = 45
                }
            };

        public MetricsModel Model => new MetricsModel() {MetricDiagrammVisibility = Visibility.Visible};

        public ChartValues<MetricValueItem> SeriesCollection { get; } = new ChartValues<MetricValueItem>()
        {
            new MetricValueItem()
            {
                Description = new MetricDescription()
                {
                    Id = 1,
                    Instance = new Instance()
                    {
                        ID = 1,
                        Source = new Source()
                            {ID = 1, Name = "testSource"},
                        Name = "testInstance"
                    },
                    Name = "testMetricGau",
                    Type = MetricType.Gauge
                },
                Value = 50,
                Created = DateTime.Now
            },
            new MetricValueItem()
            {
                Description = new MetricDescription()
                {
                    Id = 1,
                    Instance = new Instance()
                    {
                        ID = 1,
                        Source = new Source()
                            {ID = 1, Name = "testSource"},
                        Name = "testInstance"
                    },
                    Name = "testMetricGau",
                    Type = MetricType.Gauge
                },
                Value = 100,
                Created = DateTime.Now.AddMinutes(-5)
            },
            new MetricValueItem()
            {
                Description = new MetricDescription()
                {
                    Id = 1,
                    Instance = new Instance()
                    {
                        ID = 1,
                        Source = new Source()
                            {ID = 1, Name = "testSource"},
                        Name = "testInstance"
                    },
                    Name = "testMetricGau",
                    Type = MetricType.Gauge
                },
                Value = 90,
                Created = DateTime.Now.AddMinutes(-10)
            },
        };


        public long AxisUnit { get; set; } = TimeSpan.TicksPerSecond;
        public long AxisStep { get; set; } = TimeSpan.FromSeconds(1).Ticks;
        public long AxisMin  => SeriesCollection.First().Created.AddSeconds(-1).Ticks;
        public long AxisMax  => SeriesCollection.Last().Created.AddSeconds(1).Ticks;

        public Func<double, string> DateTimeFormatter
        {
            get { return value => new DateTime((long) value).ToString("mm:ss"); }
        }
    }
}
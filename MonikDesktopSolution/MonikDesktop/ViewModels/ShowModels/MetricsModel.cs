using System;
using System.Windows;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop.ViewModels.ShowModels
{
    public class MetricsModel : ShowModel
    {
        public MetricsModel()
        {
            this.ObservableForProperty(x => x.MetricTerminalMode)
               .Subscribe(_ =>
                {
                    Online                   = false;
                    MetricValuesVisibility   = MetricTerminalMode == MetricTerminalMode.Diagramm ? Visibility.Collapsed : Visibility.Visible;
                    MetricDiagrammVisibility = MetricTerminalMode == MetricTerminalMode.Diagramm ? Visibility.Visible : Visibility.Collapsed;
                });

            this.ObservableForProperty(x => x.MetricSecInterval).Subscribe(_ => Online     = false);
            this.ObservableForProperty(x => x.WindowIntervalWidth).Subscribe(_ => Online   = false);
            this.ObservableForProperty(x => x.MetricAggWindowsDepth).Subscribe(_ => Online = false);
        }

        [Reactive] public MetricTerminalMode MetricTerminalMode       { get; set; } = MetricTerminalMode.Current;
        [Reactive] public Visibility         MetricValuesVisibility   { get; set; } = Visibility.Visible;
        [Reactive] public Visibility         MetricDiagrammVisibility { get; set; } = Visibility.Collapsed;
        [Reactive] public int                MetricSecInterval        { get; set; } = 5 * 60; //for 5 minutes
        [Reactive] public int                WindowIntervalWidth      { get; set; } = 6;      //for half an hour
        [Reactive] public int                MetricAggWindowsDepth    { get; set; } = 6;      //for 6 half an hour History Chart points and 1 Current
    }

    public enum MetricTerminalMode
    {
        Current,
        TimeWindow,
        Diagramm
    }
}
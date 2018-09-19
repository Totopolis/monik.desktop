using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Windows;

namespace MonikDesktop.ViewModels.ShowModels
{
    public class MetricsModel : ShowModel
    {
        public MetricsModel()
        {
            this.ObservableForProperty(x => x.MetricTerminalMode)
               .Subscribe(_ =>
                {
                    Online = false;
                    MetricValuesVisibility = MetricTerminalMode == MetricTerminalMode.Diagram ? Visibility.Collapsed : Visibility.Visible;
                    MetricDiagrammVisibility = MetricTerminalMode == MetricTerminalMode.Diagram ? Visibility.Visible : Visibility.Collapsed;
                });
        }

        [Reactive] public MetricTerminalMode MetricTerminalMode { get; set; } = MetricTerminalMode.Current;
        [Reactive] public Visibility MetricValuesVisibility { get; set; } = Visibility.Visible;
        [Reactive] public Visibility MetricDiagrammVisibility { get; set; } = Visibility.Collapsed;
        [Reactive] public int MetricWindowDepth { get; set; } = 1; // 1 hour window depth == 12 * 5 min history bars
    }

    public enum MetricTerminalMode
    {
        Current,
        TimeWindow,
        Diagram
    }
}

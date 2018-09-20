using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;

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
                    MetricDiagramVisible = MetricTerminalMode == MetricTerminalMode.Diagram;
                });
        }

        [Reactive] public MetricTerminalMode MetricTerminalMode { get; set; } = MetricTerminalMode.Current;
        [Reactive] public bool MetricDiagramVisible { get; set; }
        [Reactive] public int MetricHistoryDepthHours { get; set; } = 1; // in hours - 1 == 12 history bars by 5 min intervals
        [Reactive] public int MetricHistorySkip5Min { get; set; } = 0; // skip some 5 min intervals
    }

    public enum MetricTerminalMode
    {
        Current,
        TimeWindow,
        Diagram
    }
}

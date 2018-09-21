using MonikDesktop.Common.ModelsApp;
using ReactiveUI;

namespace MonikDesktop.ViewModels
{
    public class NodeSource
    {
        public Source Value { get; set; }
        public ReactiveList<NodeInstance> Instances { get; set; }
    }

    public class NodeInstance
    {
        public Instance Value { get; set; }
        public ReactiveList<NodeMetric> Metrics { get; set; }
    }

    public class NodeMetric
    {
        public Metric Value { get; set; }
    }
}
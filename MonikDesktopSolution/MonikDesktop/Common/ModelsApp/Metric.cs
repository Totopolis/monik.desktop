using MonikDesktop.Common.ModelsApi;

namespace MonikDesktop.Common.ModelsApp
{
    public class Metric
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Instance Instance { get; set; }
        public MetricType Aggregation { get; set; }
    }
}

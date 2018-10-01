namespace MonikDesktop.Common.ModelsApi
{
    public class EMetric
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int InstanceID { get; set; }
        public MetricType Aggregation { get; set; }
    }
}

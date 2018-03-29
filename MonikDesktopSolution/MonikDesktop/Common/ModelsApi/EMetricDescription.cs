namespace MonikDesktop.Common.ModelsApi
{
    public class EMetricDescription
    {
        public long       Id         { get; set; }
        public string     Name       { get; set; }
        public int        InstanceId { get; set; }
        public MetricType Type       { get; set; }
    }

    public enum MetricType
    {
        Accumulator = 0,
        Gauge       = 1,
    }
}
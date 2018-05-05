using System;

namespace MonikDesktop.Common.ModelsApi
{
    public class EMetricValue
    {
        public int MetricId { get; set; }
        public double Value { get; set; }
        public DateTime Interval { get; set; }
    }
}

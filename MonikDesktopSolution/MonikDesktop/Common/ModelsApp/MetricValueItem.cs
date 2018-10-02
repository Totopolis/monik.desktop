using System;

namespace MonikDesktop.Common.ModelsApp
{
    public class MetricValueItem
    {
        public Metric Metric { get; set; }
        public double Value { get; set; }
        public bool HasValue { get; set; }
        public DateTime Interval { get; set; }
    }
}

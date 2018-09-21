using System;

namespace MonikDesktop.Common.ModelsApp
{
    public class MetricValueItem
    {
        public MetricDescription Description { get; set; }
        public double Value { get; set; }
        public DateTime Interval { get; set; }
    }
}

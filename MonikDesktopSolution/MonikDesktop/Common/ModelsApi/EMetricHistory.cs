using System;

namespace MonikDesktop.Common.ModelsApi
{
    public class EMetricHistory
    {
        public int MetricId { get; set; }
        public DateTime Interval { get; set; }
        public double[] Values { get; set; }
    }
}

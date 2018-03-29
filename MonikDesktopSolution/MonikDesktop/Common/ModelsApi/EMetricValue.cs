using System;

namespace MonikDesktop.Common.ModelsApi
{
    public class EMetricValue
    {
        public long     Id             { get; set; }
        public int      MetricId       { get; set; }
        public long     Value          { get; set; }
        public DateTime Created        { get; set; }
        public int      AggValuesCount { get; set; }
    }
}
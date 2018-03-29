using System;

namespace MonikDesktop.Common.ModelsApi
{
    public class EHistoryMetricsRequest
    {
        public long      MetricId          { get; set; }
        public int       Count             { get; set; }
        public DateTime? LastWindowCreated { get; set; }
    }
}
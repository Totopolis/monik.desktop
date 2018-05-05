using System;

namespace MonikDesktop.Common.ModelsApi
{
    public class EMetricDescription
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int InstanceId { get; set; }
        public int Aggregation { get; set; }

        public long RangeHeadId { get; set; }
        public long RangeTailId { get; set; }

        public DateTime ActualIntervalTime { get; set; }
        public long ActualId { get; set; }
    }

    public enum MetricType : int
    {
        Accumulator = 0,
        Gauge = 10,
    }
}

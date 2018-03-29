using System;
using MonikDesktop.Common.ModelsApi;

namespace MonikDesktop.Common.ModelsApp
{
    public class MetricValueItem
    {
        public MetricDescription Description    { get; set; }
        public long              Value          { get; set; }
        public DateTime          Created        { get; set; }
        public int               AggValuesCount { get; set; }

        public void AggValue()
        {
            switch (Description.Type)
            {
                case MetricType.Accumulator: break;
                case MetricType.Gauge:
                    Value = AggValuesCount != 0 ? Value / AggValuesCount : 0;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
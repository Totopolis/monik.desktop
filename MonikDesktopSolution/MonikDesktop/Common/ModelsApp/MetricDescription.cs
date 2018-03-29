using MonikDesktop.Common.ModelsApi;

namespace MonikDesktop.Common.ModelsApp
{
    public class MetricDescription
    {
        public int        Id       { get; set; }
        public string     Name     { get; set; }
        public Instance   Instance { get; set; }
        public MetricType Type     { get; set; }
    }
}
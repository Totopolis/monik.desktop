using System.Collections.Generic;
using MonikDesktop.Common.ModelsApp;

namespace MonikDesktop.Common
{
    public class SourcesCacheState
    {
        public List<Group> Groups { get; set; }
        public List<Source> Sources { get; set; }
        public List<Metric> Metrics { get; set; }
        public Dictionary<int, Instance> Instances { get; set; }
        public List<SourceItem> SourceItems { get; set; }
    }
}

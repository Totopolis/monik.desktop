using System.Collections.Generic;

namespace MonikDesktop.Common.ModelsApp
{
    public class Source
    {
        public short ID { get; set; }
        public string Name { get; set; }

        public List<Instance> Instances { get; set; }
    }
}
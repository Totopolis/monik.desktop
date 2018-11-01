using System.Collections.ObjectModel;

namespace MonikDesktop.Common.ModelsApp
{
    public class Source
    {
        public short ID { get; set; }
        public string Name { get; set; }

        public ObservableCollection<Instance> Instances { get; set; }
    }
}
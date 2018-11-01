using System.Collections.ObjectModel;

namespace MonikDesktop.Common.ModelsApp
{
    public class Group
	{
		public short ID { get; set; }
		public string Name { get; set; }
		public bool IsDefault { get; set; }

		public ObservableCollection<Instance> Instances { get; set; }
	}
}
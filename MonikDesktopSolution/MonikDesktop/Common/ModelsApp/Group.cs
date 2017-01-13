using System.Collections.Generic;

namespace MonikDesktop
{
	public class Group
	{
		public short ID { get; set; }
		public string Name { get; set; }
		public bool IsDefault { get; set; }

		public List<Instance> Instances { get; set; }
	}
}
using System;
using System.Collections.Generic;

namespace MonikDesktop
{
	public class Source
	{
		public short ID { get; set; }
		public string Name { get; set; }
	}

	public class Instance
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public Source Source { get; set; }
	}

	public class Group
	{
		public short ID { get; set; }
		public string Name { get; set; }
		public bool IsDefault { get; set; }

		public List<Instance> Instances { get; set; }
	}

	public class LogItem
	{
		public long ID { get; set; }
		public DateTime Created { get; set; }
		public string CreatedStr { get; set; }
		public DateTime Received { get; set; }
		public string ReceivedStr { get; set; }
		public byte Level { get; set; }
		public byte Severity { get; set; }
		public Instance Instance { get; set; }
		public byte Format { get; set; }
		public string Body { get; set; }
		public string Tags { get; set; }
	}
}
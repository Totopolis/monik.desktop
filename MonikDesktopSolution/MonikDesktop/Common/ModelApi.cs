using System;

namespace MonikDesktop
{
	public class ESource
	{
		public short ID { get; set; }
		public string Name { get; set; }
	}

	public class EInstance
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public int SourceID { get; set; }
	}

	public class EGroup
	{
		public short ID { get; set; }
		public string Name { get; set; }
		public bool IsDefault { get; set; }

		public int[] Instances { get; set; }
	}

	public class ELog_
	{
		public long ID { get; set; }
		public DateTime Created { get; set; }
		public DateTime Received { get; set; }
		public byte Level { get; set; }
		public byte Severity { get; set; }
		public int InstanceID { get; set; }
		public byte Format { get; set; }
		public string Body { get; set; }
		public string Tags { get; set; }
	}

	public class ELogRequest
	{
		public short[] Groups { get; set; }
		public int[] Instances { get; set; }

		public long? LastID { get; set; }
		public byte? SeverityCutoff { get; set; }
		public byte? Level { get; set; }
		public int? Top { get; set; }
	}
}
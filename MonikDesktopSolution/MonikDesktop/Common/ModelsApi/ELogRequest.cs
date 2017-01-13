namespace MonikDesktop
{
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
namespace MonikDesktop
{
	public class EGroup
	{
		public short ID { get; set; }
		public string Name { get; set; }
		public bool IsDefault { get; set; }

		public int[] Instances { get; set; }
	}
}
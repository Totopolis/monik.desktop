namespace MonikDesktop.Common.ModelsApi
{
    public class EKeepAliveRequest
    {
        public short[] Groups { get; set; } = new short[0];
        public int[] Instances { get; set; } = new int[0];
    }
}
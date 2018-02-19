using System;

namespace MonikDesktop.Common.ModelsApp
{
	public class LogItem
	{
		public long     ID          { get; set; }
		public DateTime Created     { get; set; }
		public string   CreatedStr  { get; set; }
		public DateTime Received    { get; set; }
		public string   ReceivedStr { get; set; }
		public byte     Level       { get; set; }
		public byte     Severity    { get; set; }
		public Instance Instance    { get; set; }
		public byte     Format      { get; set; }
		public string   Body        { get; set; }
		public string   Tags        { get; set; }
	}
}
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop.ViewModels
{
	public class SourceItem : ReactiveObject
	{
		public short GroupID { get; set; }
		public string GroupName { get; set; }
		public string SourceName { get; set; }
		public string InstanceName { get; set; }
		public int InstanceID { get; set; }

		[Reactive]
		public bool Checked { get; set; }
	}
}
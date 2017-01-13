using MonikDesktop.Oak;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop
{
	public class OakApplication : ReactiveObject, IOakApplication
	{
		[Reactive]
		public string ServerUrl { get; set; } = "";

		[Reactive]
		public string Title { get; set; } = "Monik Desktop";
	} //end of class
}
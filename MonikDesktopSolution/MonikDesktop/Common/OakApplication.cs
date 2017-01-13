using Doaking.Core.Oak;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop.Common
{
	public class OakApplication : ReactiveObject, IOakApplication
	{
		[Reactive]
		public string ServerUrl { get; set; } = "";

		[Reactive]
		public string Title { get; set; } = "Monik Desktop";
	} //end of class
}
using MonikDesktop.Oak;
using MonikDesktop.ViewModels;
using ReactiveUI;

namespace MonikDesktop
{
	public interface IShowWindow : IDockingWindow
	{
		ReactiveCommand StartCommand { get; set; }
		ReactiveCommand StopCommand { get; set; }

		ShowModel Model { get; }
	}
}
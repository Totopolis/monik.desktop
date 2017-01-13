using Doaking.Core.Oak;
using MonikDesktop.ViewModels;
using MonikDesktop.ViewModels.ShowModels;
using ReactiveUI;

namespace MonikDesktop.Common.Interfaces
{
	public interface IShowWindow : IDockingWindow
	{
		ReactiveCommand StartCommand { get; set; }
		ReactiveCommand StopCommand { get; set; }

		ShowModel Model { get; }
	}
}
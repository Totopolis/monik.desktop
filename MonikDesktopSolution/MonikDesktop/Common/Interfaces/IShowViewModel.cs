using MonikDesktop.ViewModels.ShowModels;
using ReactiveUI;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.Common.Interfaces
{
    public interface IShowViewModel : IViewModel
	{
		ReactiveCommand StartCommand { get; set; }
		ReactiveCommand StopCommand { get; set; }

		ShowModel Model { get; }
	}
}
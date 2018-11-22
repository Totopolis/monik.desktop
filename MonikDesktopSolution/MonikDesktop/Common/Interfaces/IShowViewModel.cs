using MonikDesktop.ViewModels.ShowModels;
using ReactiveUI;
using System.Reactive;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.Common.Interfaces
{
    public interface IShowViewModel : IViewModel
	{
		ReactiveCommand<Unit, Unit> StartCommand { get; set; }
		ReactiveCommand<Unit, Unit> StopCommand { get; set; }

		ShowModel Model { get; }
	}
}
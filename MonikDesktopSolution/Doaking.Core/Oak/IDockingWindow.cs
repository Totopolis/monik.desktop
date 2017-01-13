using ReactiveUI;

namespace MonikDesktop.Oak
{
	public interface IDockingWindow
	{
		string Title { get; set; }
		bool CanClose { get; set; }
		ReactiveCommand CloseCommand { get; set; }
	}
}
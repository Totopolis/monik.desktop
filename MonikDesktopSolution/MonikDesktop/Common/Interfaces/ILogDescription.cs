using MonikDesktop.Oak;

namespace MonikDesktop
{
	public interface ILogDescription : IDockingWindow
	{
		LogItem SelectedItem { get; set; }
	}
}
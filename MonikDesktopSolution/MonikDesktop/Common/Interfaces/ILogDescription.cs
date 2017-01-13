using Doaking.Core.Oak;
using MonikDesktop.Common.ModelsApp;

namespace MonikDesktop.Common.Interfaces
{
	public interface ILogDescription : IDockingWindow
	{
		LogItem SelectedItem { get; set; }
	}
}
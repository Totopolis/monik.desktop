using System.Windows;
using ReactiveUI;

namespace Doaking.Core.Oak
{
	public interface IDockingWindow
	{
		string          Title            { get; set; }
		bool            CanClose         { get; set; }
		ReactiveCommand CloseCommand     { get; set; }
	    bool            WindowIsEnabled  { get; set; }
    }
}
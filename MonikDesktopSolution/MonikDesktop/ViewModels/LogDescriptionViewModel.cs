using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class LogDescriptionViewModel : ViewModelBase, ILogDescriptionViewModel
	{
		public LogDescriptionViewModel()
		{
			Title = "Log Description";

		    //aShell.WhenAnyValue(x => x.SelectedWindow)
		    //    .Where(v => v is IShowWindow)
		    //    .Subscribe(v => OnSelectedWindow(v as IShowWindow));

        }


	    private void OnSelectedWindow(IShowViewModel aWindow)
	    {
	        var logsWindow = aWindow as ILogsView;

	        if (logsWindow == null)
	        {
	            WindowIsEnabled = false;
	            return;
	        }

	        WindowIsEnabled = true;
	    }

	    [Reactive] public bool            CanClose        { get; set; } = false;
        [Reactive] public ReactiveCommand CloseCommand    { get; set; } = null;
	    [Reactive] public bool            WindowIsEnabled { get; set; } = true;
        [Reactive] public LogItem         SelectedItem    { get; set; } = null;
	} //end of class
}
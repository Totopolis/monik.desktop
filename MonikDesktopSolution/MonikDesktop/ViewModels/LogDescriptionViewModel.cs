using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class LogDescriptionViewModel : ViewModelBase, ILogDescriptionViewModel
	{
		public LogDescriptionViewModel(IShell shell)
		{
			Title = "Log Description";

            shell.WhenAnyValue(x => x.SelectedView)
                .Where(v => v is IShowView)
                .Subscribe(v => OnSelectedWindow(v as IShowView));
        }

	    private void OnSelectedWindow(IShowView aWindow)
	    {
	        IsEnabled = aWindow is ILogsView;
	    }

        [Reactive] public LogItem         SelectedItem    { get; set; } = null;
	} //end of class
}
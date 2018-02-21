using System;
using System.Reactive.Linq;
using System.Windows;
using Doaking.Core.Oak;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop.ViewModels
{
	public class LogDescriptionViewModel : ReactiveObject, ILogDescription
	{
		public LogDescriptionViewModel(Shell aShell)
		{
			Title = "Log Description";

		    aShell.WhenAnyValue(x => x.SelectedWindow)
		        .Where(v => v is IShowWindow)
		        .Subscribe(v => OnSelectedWindow(v as IShowWindow));

        }


	    private void OnSelectedWindow(IShowWindow aWindow)
	    {
	        var logsWindow = aWindow as ILogsWindow;

	        if (logsWindow == null)
	        {
	            WindowIsEbabled = false;
	            return;
	        }

	        WindowIsEbabled = true;
	    }

	    [Reactive] public bool            CanClose        { get; set; } = false;
        [Reactive] public ReactiveCommand CloseCommand    { get; set; } = null;
	    [Reactive] public bool            WindowIsEbabled { get; set; } = true;
        [Reactive] public LogItem         SelectedItem    { get; set; } = null;
        [Reactive] public string          Title           { get; set; }
	} //end of class
}
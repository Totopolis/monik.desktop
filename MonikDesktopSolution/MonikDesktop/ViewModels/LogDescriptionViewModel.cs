using MonikDesktop.Common.ModelsApp;
using MonikDesktop.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class LogDescriptionViewModel : ViewModelBase
    {
        public LogDescriptionViewModel(IShell shell)
        {
            Title = "Log Description";

            shell.WhenAnyValue(x => x.SelectedView)
                .Where(v => !(v is IToolView))
                .Subscribe(v => IsEnabled = v is LogsView);
        }

        [Reactive] public LogItem SelectedItem { get; set; } = null;
    } //end of class
}
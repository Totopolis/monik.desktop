using MonikDesktop.Common.ModelsApp;
using MonikDesktop.ViewModels.ShowModels;
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
        private IDisposable _subscription;

        public LogDescriptionViewModel(IShell shell)
        {
            Title = "Log Description";

            shell.WhenAnyValue(x => x.SelectedView)
                .Where(v => !(v is IToolView))
                .Subscribe(v =>
                {
                    if (v is LogsView logsView)
                    {
                        UpdateModel(logsView.ShowViewModel.Model as LogsModel);
                        IsEnabled = true;
                    }
                    else
                        IsEnabled = false;
                });
        }

        [Reactive] public LogItem SelectedItem { get; set; } = null;

        private void UpdateModel(LogsModel model)
        {
            _subscription?.Dispose();
            _subscription = model
                .WhenAnyValue(x => x.SelectedItem)
                .Subscribe(x => SelectedItem = x);
        }

        protected override void DisposeInternals()
        {
            base.DisposeInternals();
            _subscription?.Dispose();
        }
    } //end of class
}
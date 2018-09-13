using MonikDesktop.Common.Interfaces;
using MonikDesktop.Properties;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Windows;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class StartupViewModel : ViewModelBase, IStartupViewModel
    {
        private readonly IShell _shell;
        private bool _toolsShown;

        public StartupViewModel(IShell shell, IAppModel app, IDockWindow main)
        {
            _shell = shell;
            
            Title = "App settings";
            App = app;

            app.WhenAnyValue(x => x.Title)
                .Subscribe(x => main.Title = x);

            app.ObservableForProperty(x => x.ServerUrl)
               .Subscribe(v =>
                {
                    Settings.Default.ServerUrl = v.Value;
                    Settings.Default.Save();
                });

            var canNew = app.WhenAny(x => x.ServerUrl, x => !string.IsNullOrWhiteSpace(x.Value));
            NewLogCommand       = ReactiveCommand.Create(NewLog,       canNew);
            NewKeepAliveCommand = ReactiveCommand.Create(NewKeepAlive, canNew);
            NewMetricsCommand   = ReactiveCommand.Create(NewMetrics,   canNew);
        }

        public ReactiveCommand NewLogCommand       { get; set; }
        public ReactiveCommand NewKeepAliveCommand { get; set; }
        public ReactiveCommand NewMetricsCommand   { get; set; }
        public ReactiveCommand CloseCommand        { get; set; } = null;

        [Reactive] public bool       CanClose        { get; set; } = false;
        [Reactive] public bool       WindowIsEnabled { get; set; } = true;
        [Reactive] public Visibility ShowSpinner     { get; set; } = Visibility.Collapsed;

        private void NewLog()
        {
            // TODO: check server url

            ShowSpinner = Visibility.Visible;

            _shell.ShowView<ILogsView>();

            ShowTools();

            ShowSpinner = Visibility.Collapsed;
        }

        private void NewKeepAlive()
        {
            // TODO: check server url

            ShowSpinner = Visibility.Visible;

            _shell.ShowView<IKeepAliveView>();

            ShowTools();

            ShowSpinner = Visibility.Collapsed;
        }

        private void NewMetrics()
        {
            // TODO: check server url

            ShowSpinner = Visibility.Visible;

            _shell.ShowView<IMetricsView>();

            ShowTools();

            ShowSpinner = Visibility.Collapsed;
        }

        private void ShowTools()
        {
            if (_toolsShown)
                return;

            _toolsShown = true;
            _shell.ShowTool<IPropertiesView>();
            _shell.ShowTool<ISourcesView>();
            _shell.ShowTool<ILogDescriptionView>();
        }

        public IAppModel App { get; }
    }
}
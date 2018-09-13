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

        public StartupViewModel(IAppModel app, IShell shell)
        {
            _shell = shell;
            
            Title = "App settings";

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
            _shell.ShowTool<IPropertiesView>();
            _shell.ShowTool<ISourcesView>();
            _shell.ShowTool<ILogDescriptionView>();

//            _shell.SelectedWindow = log;

            ShowSpinner = Visibility.Collapsed;
        }

        private void NewKeepAlive()
        {
            // TODO: check server url

            ShowSpinner = Visibility.Visible;

            _shell.ShowView<IKeepAliveView>();
            _shell.ShowTool<IPropertiesView>();
            _shell.ShowTool<ISourcesView>();
            _shell.ShowTool<ILogDescriptionView>();

//            _shell.SelectedWindow = keepAlive;

            ShowSpinner = Visibility.Collapsed;
        }

        private void NewMetrics()
        {
            // TODO: check server url

            ShowSpinner = Visibility.Visible;

            _shell.ShowView<IMetricsView>();
            _shell.ShowTool<IPropertiesView>();
            _shell.ShowTool<ISourcesView>();
            _shell.ShowTool<ILogDescriptionView>();

//            _shell.SelectedWindow = metrics;


            ShowSpinner = Visibility.Collapsed;
        }
    }
}
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Properties;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class StartupViewModel : ViewModelBase, IStartupViewModel
    {
        private readonly IShell _shell;
        private readonly ISourcesCache _cache;
        private bool _isInitialized;

        public StartupViewModel(IShell shell, ISourcesCache cache, IAppModel app, IDockWindow main)
        {
            _shell = shell;
            _cache = cache;
            
            Title = "App settings";
            CanClose = false;
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

        private async Task Initialize()
        {
            _isInitialized = true;

            IsBusy = true;

            // TODO: check server url
            await Task.Run(() => _cache.Reload());

            ShowTools();

            IsBusy = false;
        }

        private async Task NewLog()
        {
            if (!_isInitialized)
                await Initialize();

            _shell.ShowView<ILogsView>();
        }

        private async Task NewKeepAlive()
        {
            if (!_isInitialized)
                await Initialize();

            _shell.ShowView<IKeepAliveView>();
        }

        private async Task NewMetrics()
        {
            if (!_isInitialized)
                await Initialize();

            _shell.ShowView<IMetricsView>();
        }

        private void ShowTools()
        {
            _shell.ShowTool<IPropertiesView>();
            _shell.ShowTool<ISourcesView>();
            _shell.ShowTool<ILogDescriptionView>();
        }

        public IAppModel App { get; }
    }
}
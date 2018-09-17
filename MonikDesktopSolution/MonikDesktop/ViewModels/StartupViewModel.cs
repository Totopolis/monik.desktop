using MonikDesktop.Common.Interfaces;
using MonikDesktop.Properties;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class StartupViewModel : ViewModelBase, IStartupViewModel
    {
        public ReactiveList<string> ServerUrls { get; } = new ReactiveList<string>();
        [Reactive] public string AppTitle { get; set; } = "Monik Desktop";

        private readonly IShell _shell;
        private readonly ISourcesCache _cache;
        private bool _isInitialized;

        public StartupViewModel(IShell shell, ISourcesCache cache, IAppModel app)
        {
            _shell = shell;
            _cache = cache;
            
            Title = "App settings";
            CanClose = false;
            App = app;

            this.WhenAnyValue(x => x.AppTitle)
                .Subscribe(x => shell.Title = x);

            var urls = Settings.Default.ServerUrl.Split(';');
            ServerUrls.AddRange(urls);
            app.ServerUrl = urls.FirstOrDefault();

            ServerUrls.Changed.Subscribe(x =>
            {
                Settings.Default.ServerUrl = string.Join(";", ServerUrls);
                Settings.Default.Save();
            });

            var canNew = app.WhenAny(x => x.ServerUrl, x => !string.IsNullOrWhiteSpace(x.Value));
            NewLogCommand       = ReactiveCommand.Create(NewLog,       canNew);
            NewKeepAliveCommand = ReactiveCommand.Create(NewKeepAlive, canNew);
            NewMetricsCommand   = ReactiveCommand.Create(NewMetrics,   canNew);

            RemoveUrlCommand    = ReactiveCommand.Create<string>(RemoveUrl);
        }

        public ReactiveCommand NewLogCommand       { get; set; }
        public ReactiveCommand NewKeepAliveCommand { get; set; }
        public ReactiveCommand NewMetricsCommand   { get; set; }

        public ReactiveCommand RemoveUrlCommand    { get; set; }

        public string UpdateServerUrl
        {
            set
            {
                if (App.ServerUrl != null)
                {
                    // move to the top
                    var index = ServerUrls.IndexOf(App.ServerUrl);
                    if (index != 0)
                    {
                        using (ServerUrls.SuppressChangeNotifications())
                        {
                            ServerUrls.RemoveAt(index);
                            ServerUrls.Insert(0, App.ServerUrl);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(value))
                {
                    ServerUrls.Insert(0, value);
                    App.ServerUrl = value;
                }
            }
        }

        public void RemoveUrl(string url)
        {
            ServerUrls.Remove(url);
        }

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
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
        public ReactiveList<string> AuthTokens { get; } = new ReactiveList<string>();
        public ReactiveList<Uri> ServerUrls { get; } = new ReactiveList<Uri>();
        [Reactive] public string AppTitle { get; set; } = "Monik Desktop";

        private readonly IShell _shell;
        private readonly ISourcesCache _cache;
        private bool _isInitialized;
        private bool _isToolsShown;

        public StartupViewModel(IShell shell, ISourcesCache cache, IAppModel app)
        {
            _shell = shell;
            _cache = cache;
            
            Title = "App settings";
            CanClose = false;
            App = app;

            // Title

            this.WhenAnyValue(x => x.AppTitle)
                .Subscribe(x => shell.Title = x);

            // Server Urls

            var urls = Settings.Default.ServerUrl
                .Split(';')
                .Select(x => Uri.TryCreate(x, UriKind.Absolute, out var result) ? result : null as Uri)
                .Where(x => x != null)
                .ToArray();

            ServerUrls.AddRange(urls);
            app.ServerUrl = urls.FirstOrDefault();

            ServerUrls.Changed.Subscribe(x =>
            {
                Settings.Default.ServerUrl = string.Join(";", ServerUrls);
                Settings.Default.Save();
            });

            // Authorization Tokens

            var tokens = Settings.Default.AuthToken
                .Split(';')
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();

            AuthTokens.AddRange(tokens);
            App.AuthToken = tokens.FirstOrDefault();

            AuthTokens.Changed.Subscribe(x =>
            {
                Settings.Default.AuthToken = string.Join(";", AuthTokens);
                Settings.Default.Save();
            });

            // Create Commands

            var hasUrl = app.WhenAny(x => x.ServerUrl, x => x.Value != null);
            NewLogCommand       = ReactiveCommand.Create(NewLog,       hasUrl);
            NewKeepAliveCommand = ReactiveCommand.Create(NewKeepAlive, hasUrl);
            NewMetricsCommand   = ReactiveCommand.Create(NewMetrics,   hasUrl);

            RemoveEntitiesCommand = ReactiveCommand.Create(RemoveEntities, hasUrl);
            ManageGroupsCommand = ReactiveCommand.Create(ManageGroups, hasUrl);

            RemoveUrlCommand    = ReactiveCommand.Create<Uri>(url => ServerUrls.Remove(url));
            RemoveAuthTokenCommand = ReactiveCommand.Create<string>(token => AuthTokens.Remove(token));
        }

        public ReactiveCommand NewLogCommand       { get; set; }
        public ReactiveCommand NewKeepAliveCommand { get; set; }
        public ReactiveCommand NewMetricsCommand   { get; set; }

        public ReactiveCommand RemoveEntitiesCommand { get; set; }
        public ReactiveCommand ManageGroupsCommand { get; set; }

        public ReactiveCommand RemoveUrlCommand    { get; set; }
        public ReactiveCommand RemoveAuthTokenCommand { get; set; }

        public string UpdateServerUrl
        {
            set
            {
                // will throw if Uri is incorrect
                // and will be made red by ValidatesOnExceptions
                var val = new Uri(value);

                if (App.ServerUrl != null && App.ServerUrl == val)
                {
                    // move to the top
                    var index = ServerUrls.IndexOf(App.ServerUrl);
                    if (index != 0)
                    {
                        using (ServerUrls.SuppressChangeNotifications())
                            (ServerUrls[index], ServerUrls[0]) = (ServerUrls[0], ServerUrls[index]);
                    }
                }
                else
                {
                    ServerUrls.Insert(0, val);
                    App.ServerUrl = val;
                }
            }
        }

        public string UpdateAuthToken
        {
            set
            {
                if (App.AuthToken != null && App.AuthToken == value)
                {
                    // move to the top
                    var index = AuthTokens.IndexOf(App.AuthToken);
                    if (index != 0)
                    {
                        using (AuthTokens.SuppressChangeNotifications())
                            (AuthTokens[index], AuthTokens[0]) = (AuthTokens[0], AuthTokens[index]);
                    }
                }
                else
                {
                    AuthTokens.Insert(0, value);
                    App.AuthToken = value;
                }
            }
        }

        private async Task Initialize()
        {
            _isInitialized = true;

            IsBusy = true;

            await Task.Run(() => _cache.Reload());

            IsBusy = false;
        }

        private async Task NewLog()
        {
            if (!_isInitialized)
                await Initialize();

            _shell.ShowView<ILogsView>();
            ShowTools();
        }

        private async Task NewKeepAlive()
        {
            if (!_isInitialized)
                await Initialize();

            _shell.ShowView<IKeepAliveView>();
            ShowTools();
        }

        private async Task NewMetrics()
        {
            if (!_isInitialized)
                await Initialize();

            _shell.ShowView<IMetricsView>();
            ShowTools();
        }

        private void ShowTools()
        {
            if (_isToolsShown)
                return;

            _isToolsShown = true;
            _shell.ShowTool<IPropertiesView>();
            _shell.ShowTool<ISourcesView>();
            _shell.ShowTool<ILogDescriptionView>();
        }

        private async Task RemoveEntities()
        {
            if (!_isInitialized)
                await Initialize();

            _shell.ShowView<IRemoveEntitiesView>();
        }

        private async Task ManageGroups()
        {
            if (!_isInitialized)
                await Initialize();

            _shell.ShowView<IManageGroupsView>();
        }

        public IAppModel App { get; }
    }
}
using DynamicData;
using MahApps.Metro;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using MonikDesktop.Common.Config;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class StartupViewModel : ViewModelBase
    {
        [Reactive] public string AuthToken { get; set; }
        private SourceList<string> AuthTokensSource { get; }
        private readonly ReadOnlyObservableCollection<string> _authTokens;
        public ReadOnlyObservableCollection<string> AuthTokens => _authTokens;
        [Reactive] public Uri ServerUrl { get; set; }
        private SourceList<Uri> ServerUrlsSource { get; }
        private readonly ReadOnlyObservableCollection<Uri> _serverUrls;
        public ReadOnlyObservableCollection<Uri> ServerUrls => _serverUrls;
        [Reactive] public string AppTitle { get; set; }
        [Reactive] public string AuthTokenDescription { get; set; }

        public List<string> Accents { get; } = ThemeManager.ColorSchemes.Select(a => a.Name).ToList();
        [Reactive] public string Accent { get; set; }
        [Reactive] public bool IsDark { get; set; }

        private readonly IShell _shell;
        private readonly ISourcesCacheProvider _cacheProvider;
        private readonly IDockWindow _window;
        private bool _isToolsShown;

        public StartupViewModel(IShell shell, ISourcesCacheProvider cacheProvider, IDockWindow window)
        {
            _shell = shell;
            _cacheProvider = cacheProvider;
            _window = window;

            var appConfigStorage = new AppConfigStorage();
            var config = appConfigStorage.Load();
            
            Title = "App settings";

            // Title

            AppTitle = shell.Title;
            this.WhenAnyValue(x => x.AppTitle)
                .Subscribe(x => shell.Title = x);

            // Theme

            Accent = config.Accent ?? "Blue";
            IsDark = config.IsDark;
            UpdateTheme();

            this.ObservableForProperty(x => x.Accent)
                .Subscribe(x =>
                {
                    config.Accent = x.Value;
                    appConfigStorage.Save(config);
                    UpdateTheme();
                });
            this.ObservableForProperty(x => x.IsDark)
                .Subscribe(x =>
                {
                    config.IsDark = x.Value;
                    appConfigStorage.Save(config);
                    UpdateTheme();
                });

            // Server Urls

            var urls = config.ServerUrl?
                .Split(';')
                .Select(x => Uri.TryCreate(x, UriKind.Absolute, out var result) ? result : null as Uri)
                .Where(x => x != null)
                .ToArray();

            ServerUrlsSource = new SourceList<Uri>();
            if (urls != null)
            {
                ServerUrlsSource.AddRange(urls);
                ServerUrl = urls.First();
            }

            ServerUrlsSource
                .Connect()
                .ObserveOnDispatcher()
                .Bind(out _serverUrls)
                .ToCollection()
                .Subscribe(items =>
                {
                    config.ServerUrl = string.Join(";", items);
                    appConfigStorage.Save(config);
                });

            this.WhenAnyValue(x => x.ServerUrl)
                .Skip(1)
                .Subscribe(_ => UpdateSourcesCache());

            // Authorization Tokens

            var tokens = config.AuthToken?
                .Split(';')
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();

            AuthTokensSource = new SourceList<string>();
            if (tokens != null)
            {
                AuthTokensSource.AddRange(tokens);
                AuthToken = tokens.First();
            }

            AuthTokensSource
                .Connect()
                .ObserveOnDispatcher()
                .Bind(out _authTokens)
                .ToCollection()
                .Subscribe(items =>
                {
                    config.AuthToken = string.Join(";", items);
                    appConfigStorage.Save(config);
                });

            this.WhenAnyValue(x => x.AuthToken)
                .Skip(1)
                .Subscribe(_ => UpdateSourcesCache());

            // Create Commands

            var hasUrl = this.WhenAny(x => x.ServerUrl, x => x.Value != null);
            NewLogCommand       = CreateCommandWithInit(NewLog,       hasUrl);
            NewKeepAliveCommand = CreateCommandWithInit(NewKeepAlive, hasUrl);
            NewMetricsCommand   = CreateCommandWithInit(NewMetrics,   hasUrl);

            RemoveEntitiesCommand = CreateCommandWithInit(RemoveEntities, hasUrl);
            ManageGroupsCommand = CreateCommandWithInit(ManageGroups, hasUrl);

            RemoveUrlCommand = ReactiveCommand.Create<Uri>(url => ServerUrlsSource.Remove(url));
            RemoveAuthTokenCommand = ReactiveCommand.Create<string>(token => AuthTokensSource.Remove(token));

            RefreshCommand = ReactiveCommand.CreateFromTask(Refresh, hasUrl);

            UpdateSourcesCache();
        }

        public ReactiveCommand<Unit, Unit> NewLogCommand       { get; set; }
        public ReactiveCommand<Unit, Unit> NewKeepAliveCommand { get; set; }
        public ReactiveCommand<Unit, Unit> NewMetricsCommand   { get; set; }

        public ReactiveCommand<Unit, Unit> RemoveEntitiesCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ManageGroupsCommand { get; set; }

        public ReactiveCommand<Uri, Unit> RemoveUrlCommand    { get; set; }
        public ReactiveCommand<string, Unit> RemoveAuthTokenCommand { get; set; }

        public ReactiveCommand<Unit, Unit> RefreshCommand { get; set; }

        public void UpdateSourcesCache()
        {
            _cacheProvider.SetCurrentCacheSource(ServerUrl);
            _cacheProvider.CurrentCache.Service.AuthToken = AuthToken;
        }

        public string UpdateServerUrl
        {
            get => ServerUrl?.ToString();
            set
            {
                // will throw if Uri is incorrect
                // and will be made red by ValidatesOnExceptions
                var val = new Uri(value);

                if (ServerUrl != null && ServerUrl == val)
                {
                    // move to the top
                    var index = ServerUrlsSource.Items.IndexOf(ServerUrl);
                    if (index != 0)
                        ServerUrlsSource.Move(index, 0);
                }
                else
                {
                    ServerUrlsSource.Insert(0, val);
                    ServerUrl = val;
                }
            }
        }

        public string UpdateAuthToken
        {
            get => AuthToken;
            set
            {
                try
                {
                    // will throw if Token is not valid JWT token
                    // and will be made red by ValidatesOnExceptions
                    var token = new JwtSecurityTokenHandler().ReadJwtToken(value);
                    if (long.Parse(token.Claims.First(cl => cl.Type == "exp").Value) < DateTimeOffset.Now.ToUnixTimeSeconds())
                        throw new ArgumentException("Expired");

                    AuthTokenDescription = token.Payload.SerializeToJson();
                }
                catch
                {
                    AuthTokenDescription = null;
                    throw;
                }

                if (AuthToken != null && AuthToken == value)
                {
                    // move to the top
                    var index = AuthTokensSource.Items.IndexOf(AuthToken);
                    if (index != 0)
                        AuthTokensSource.Move(index, 0);
                }
                else if (!string.IsNullOrEmpty(value))
                {
                    AuthTokensSource.Insert(0, value);
                    AuthToken = value;
                }
            }
        }

        private ReactiveCommand<Unit, Unit> CreateCommandWithInit(Action act, IObservable<bool> canExecute)
        {
            return ReactiveCommand.CreateFromTask(WrapInInit(act), canExecute);
        }

        private Func<Task> WrapInInit(Action act)
        {
            return async () =>
            {
                if (await CheckCacheLoaded())
                    act();
            };
        }

        private async Task<bool> CheckCacheLoaded()
        {
            if (_cacheProvider.CurrentCache.IsLoaded)
                return true;

            var cacheLoaded = await TryToLoadCache();
            _cacheProvider.CurrentCache.IsLoaded = cacheLoaded;
            return cacheLoaded;
        }

        private async Task<bool> TryToLoadCache()
        {
            try
            {
                IsBusy = true;
                await _cacheProvider.CurrentCache.Load();
                return true;
            }
            catch (WebException e)
            {
                _window.ShowWebExceptionMessage(e);
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string CurrentCacheUrl => _cacheProvider.CurrentCache.Service.ServerUrl.ToString();


        private async Task Refresh()
        {
            await TryToLoadCache();
        }

        private void NewLog()
        {
            ShowTools();
            _shell.ShowView<LogsView>(new ViewRequest($"logs_{CurrentCacheUrl}"));
        }

        private void NewKeepAlive()
        {
            ShowTools();
            _shell.ShowView<KeepAliveView>(new ViewRequest($"keep-alive_{CurrentCacheUrl}"));
        }

        private void NewMetrics()
        {
            ShowTools();
            _shell.ShowView<MetricsView>(new ViewRequest($"metrics_{CurrentCacheUrl}"));
        }

        private void ShowTools()
        {
            if (!_isToolsShown)
            {
                _isToolsShown = true;
                _shell.ShowTool<PropertiesView>(new ViewRequest("props"));
                _shell.ShowTool<SourcesView>();
                _shell.ShowTool<LogDescriptionView>();
            }

            //make Properties active
            _shell.ShowTool<PropertiesView>(new ViewRequest("props"));
        }

        private void RemoveEntities()
        {
            _shell.ShowView<RemoveEntitiesView>(new ViewRequest($"remove-entities_{CurrentCacheUrl}"));
            _shell.ShowTool<StartupView>(new ViewRequest("startup"));
        }

        private void ManageGroups()
        {
            _shell.ShowView<ManageGroupsView>(new ViewRequest($"manage-groups_{CurrentCacheUrl}"));
            _shell.ShowTool<StartupView>(new ViewRequest("startup"));
        }

        private void UpdateTheme()
        {
            ThemeManager.ChangeTheme(Application.Current,
                IsDark ? ThemeManager.BaseColorDark : ThemeManager.BaseColorLight,
                Accent);
        }
    }
}

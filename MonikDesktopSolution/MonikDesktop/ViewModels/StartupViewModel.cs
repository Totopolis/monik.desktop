using Autofac;
using MahApps.Metro;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Properties;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class StartupViewModel : ViewModelBase, IStartupViewModel
    {
        public ReactiveList<string> AuthTokens { get; } = new ReactiveList<string>();
        public ReactiveList<Uri> ServerUrls { get; } = new ReactiveList<Uri>();
        public IAppModel App { get; }
        [Reactive] public string AppTitle { get; set; } = "Monik Desktop";
        [Reactive] public string AuthTokenDescription { get; set; }

        public ReactiveList<string> Accents { get; } = new ReactiveList<string>(ThemeManager.Accents.Select(a => a.Name));
        [Reactive] public string Accent { get; set; }
        [Reactive] public bool IsDark { get; set; }

        private readonly IShell _shell;
        private readonly ISourcesCache _cache;
        private readonly IDockWindow _window;
        private bool _isInitialized;
        private bool _isToolsShown;

        public StartupViewModel(IShell shell, ISourcesCache cache, IAppModel app, IDockWindow window)
        {
            _shell = shell;
            _cache = cache;
            _window = window;
            
            Title = "App settings";
            App = app;

            // Title

            this.WhenAnyValue(x => x.AppTitle)
                .Subscribe(x => shell.Title = x);

            // Theme

            Accent = Settings.Default.Accent;
            IsDark = Settings.Default.IsDark;
            UpdateTheme(false);

            this.ObservableForProperty(x => x.Accent)
                .Subscribe(_ => UpdateTheme());
            this.ObservableForProperty(x => x.IsDark)
                .Subscribe(_ => UpdateTheme());

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
            NewLogCommand       = CreateCommandWithInit(NewLog,       hasUrl);
            NewKeepAliveCommand = CreateCommandWithInit(NewKeepAlive, hasUrl);
            NewMetricsCommand   = CreateCommandWithInit(NewMetrics,   hasUrl);

            RemoveEntitiesCommand = CreateCommandWithInit(RemoveEntities, hasUrl);
            ManageGroupsCommand = CreateCommandWithInit(ManageGroups, hasUrl);

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
                else if (!string.IsNullOrEmpty(value))
                {
                    AuthTokens.Insert(0, value);
                    App.AuthToken = value;
                }
            }
        }

        private ReactiveCommand CreateCommandWithInit(Action act, IObservable<bool> canExecute)
        {
            return ReactiveCommand.Create(WrapInInit(act), canExecute);
        }

        private Func<Task> WrapInInit(Action act)
        {
            return async () =>
            {
                if (_isInitialized || await Initialize())
                    act();
            };
        }

        private async Task<bool> Initialize()
        {
            try
            {
                IsBusy = true;
                await Task.Run(() => _cache.Reload());
                _isInitialized = true;
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

        private void NewLog()
        {
            _shell.ShowView<ILogsView>();
            ShowTools();
        }

        private void NewKeepAlive()
        {
            _shell.ShowView<IKeepAliveView>();
            ShowTools();
        }

        private void NewMetrics()
        {
            _shell.ShowView<IMetricsView>();
            ShowTools();
        }

        private void ShowTools()
        {
            if (!_isToolsShown)
            {
                _isToolsShown = true;
                _shell.ShowTool<IPropertiesView>();
                _shell.ShowTool<ISourcesView>();
                _shell.ShowTool<ILogDescriptionView>();
            }

            _shell.SelectedView = _shell.Container.Resolve<IPropertiesView>();
        }

        private void RemoveEntities()
        {
            _shell.ShowView<IRemoveEntitiesView>();
            _shell.SelectedView = _shell.Container.Resolve<IStartupView>();
        }

        private void ManageGroups()
        {
            _shell.ShowView<IManageGroupsView>();
            _shell.SelectedView = _shell.Container.Resolve<IStartupView>();
        }

        private void UpdateTheme(bool needToSave = true)
        {
            if (needToSave)
            {
                Settings.Default.Accent = Accent;
                Settings.Default.IsDark = IsDark;
                Settings.Default.Save();
            }

            ThemeManager.ChangeAppStyle(Application.Current,
                ThemeManager.GetAccent(Accent),
                ThemeManager.GetAppTheme(IsDark ? "BaseDark" : "BaseLight"));
        }
    }
}
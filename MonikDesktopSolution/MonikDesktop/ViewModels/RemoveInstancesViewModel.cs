using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;
using MonikDesktop.Properties;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class RemoveInstancesViewModel : ViewModelBase, IRemoveInstancesViewModel
    {
        private readonly ISourcesCache _cache;
        private readonly IMonikService _service;

        public RemoveInstancesViewModel(ISourcesCache cache, IMonikService service, IAppModel app)
        {
            _cache = cache;
            _service = service;

            Title = "Remove Instances";

            var urls = Settings.Default.AuthToken
                .Split(';')
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();

            AuthTokens.AddRange(urls);
            app.AuthToken = urls.FirstOrDefault();

            AuthTokens.Changed.Subscribe(x =>
            {
                Settings.Default.ServerUrl = string.Join(";", AuthTokens);
                Settings.Default.Save();
            });

            RemoveAuthTokenCommand = ReactiveCommand.Create<string>(token => AuthTokens.Remove(token));

            this.ObservableForProperty(x => x.FilterText)
                .Throttle(TimeSpan.FromSeconds(0.7), RxApp.MainThreadScheduler)
                .Subscribe(v => Filter(v.Value));

            Refresh();
        }

        public ReactiveList<string> AuthTokens { get; } = new ReactiveList<string>();
        public ReactiveCommand RemoveAuthTokenCommand { get; set; }

        [Reactive] public string FilterText { get; set; }

        public ReactiveList<Instance> InstancesFiltered { get; set; } = new ReactiveList<Instance>();
        public ReactiveList<Instance> InstancesList { get; set; } = new ReactiveList<Instance>();

        private void Refresh()
        {
            InstancesList.Clear();
            InstancesList.AddRange(_cache.Instances);
            InstancesFiltered.Clear();
            InstancesFiltered.AddRange(InstancesList);
        }

        private void Filter(string filter)
        {
            filter = filter.ToLower();
            InstancesFiltered.Clear();

            IEnumerable<Instance> tmp;

            if (string.IsNullOrWhiteSpace(filter))
                tmp = InstancesList;
            else
                tmp = InstancesList
                    .Where(x => x.Name.ToLower().Contains(filter) ||
                                x.Source.Name.ToLower().Contains(filter));

            InstancesFiltered.AddRange(tmp);
        }

    }
}

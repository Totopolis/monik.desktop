using DynamicData;
using MonikDesktop.Common;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class RemoveEntitiesViewModel : ViewModelBase
    {
        private readonly ISourcesCache _cache;
        private readonly IDockWindow _window;

        public RemoveEntitiesViewModel(ISourcesCacheProvider cacheProvider, IDockWindow window)
        {
            _cache = cacheProvider.CurrentCache;
            _window = window;

            Title = "Remove Instances";

            RemoveSourceCommand = ReactiveCommand.Create<Source>(RemoveSource);
            RemoveInstanceCommand = ReactiveCommand.Create<Instance>(RemoveInstance);
            RemoveMetricCommand = ReactiveCommand.Create<Metric>(RemoveMetric);

            var filter = this.ObservableForProperty(x => x.FilterText)
                .Throttle(TimeSpan.FromSeconds(0.7), RxApp.MainThreadScheduler)
                .Publish();

            var dynamicFilterSource = filter
                .Select(v => Filters.CreateFilterSource(v.Value));
            var dynamicFilterInstance = filter
                .Select(v => Filters.CreateFilterInstance(v.Value));
            var dynamicFilterMetric = filter
                .Select(v => Filters.CreateFilterMetric(v.Value));
            
            filter.Connect()
                .DisposeWith(Disposables);

            _cache.Sources
                .Connect()
                .Filter(dynamicFilterSource)
                .Bind(out _sources)
                .Subscribe()
                .DisposeWith(Disposables);

            _cache.Instances
                .Connect()
                .Filter(dynamicFilterInstance)
                .Bind(out _instances)
                .Subscribe()
                .DisposeWith(Disposables);

            _cache.Metrics
                .Connect()
                .Filter(dynamicFilterMetric)
                .Bind(out _metrics)
                .Subscribe()
                .DisposeWith(Disposables);
        }

        public ReactiveCommand RemoveSourceCommand { get; set; }
        public ReactiveCommand RemoveInstanceCommand { get; set; }
        public ReactiveCommand RemoveMetricCommand { get; set; }

        [Reactive] public string FilterText { get; set; }

        private readonly ReadOnlyObservableCollection<Source> _sources;
        public ReadOnlyObservableCollection<Source> Sources => _sources;
        private readonly ReadOnlyObservableCollection<Instance> _instances;
        public ReadOnlyObservableCollection<Instance> Instances => _instances;
        private readonly ReadOnlyObservableCollection<Metric> _metrics;
        public ReadOnlyObservableCollection<Metric> Metrics => _metrics;
        
        private void RemoveSource(Source v)
        {
            try
            {
                _cache.RemoveSource(v);
            }
            catch (WebException e)
            {
                ShowPopupWebException(e);
            }
        }

        private void RemoveInstance(Instance v)
        {
            try
            {
                _cache.RemoveInstance(v);
            }
            catch (WebException e)
            {
                ShowPopupWebException(e);
            }
        }

        private void RemoveMetric(Metric v)
        {
            try
            {
                _cache.RemoveMetric(v);
            }
            catch (WebException e)
            {
                ShowPopupWebException(e);
            }
        }

        private void ShowPopupWebException(WebException e)
        {
            _window.ShowWebExceptionMessage(e);
        }
    }
}

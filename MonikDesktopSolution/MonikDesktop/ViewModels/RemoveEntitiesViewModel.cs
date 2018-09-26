﻿using MonikDesktop.Common;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class RemoveEntitiesViewModel : ViewModelBase, IRemoveEntitiesViewModel
    {
        private readonly ISourcesCache _cache;
        private string _filter;

        public RemoveEntitiesViewModel(ISourcesCache cache)
        {
            _cache = cache;

            Title = "Remove Instances";

            RemoveNodeSourceCommand = ReactiveCommand.Create<NodeSource>(RemoveNodeSource);
            RemoveNodeInstanceCommand = ReactiveCommand.Create<NodeInstance>(RemoveNodeInstance);
            RemoveNodeMetricCommand = ReactiveCommand.Create<NodeMetric>(RemoveNodeMetric);
            RemoveSourceCommand = ReactiveCommand.Create<Source>(v => RemoveSource(v));
            RemoveInstanceCommand = ReactiveCommand.Create<Instance>(v => RemoveInstance(v));
            RemoveMetricCommand = ReactiveCommand.Create<Metric>(v => RemoveMetric(v));

            this.ObservableForProperty(x => x.FilterText)
                .Throttle(TimeSpan.FromSeconds(0.7), RxApp.MainThreadScheduler)
                .Subscribe(v => Filter(v.Value));

            Refresh();
        }

        public ReactiveCommand RemoveNodeSourceCommand { get; set; }
        public ReactiveCommand RemoveNodeInstanceCommand { get; set; }
        public ReactiveCommand RemoveNodeMetricCommand { get; set; }
        public ReactiveCommand RemoveSourceCommand { get; set; }
        public ReactiveCommand RemoveInstanceCommand { get; set; }
        public ReactiveCommand RemoveMetricCommand { get; set; }

        [Reactive] public string FilterText { get; set; }

        public ReactiveList<NodeSource> SourcesTree { get; set; } = new ReactiveList<NodeSource>();

        public ReactiveList<Source> SourcesList { get; set; } = new ReactiveList<Source>();
        public ReactiveList<Source> SourcesFiltered { get; set; } = new ReactiveList<Source>();
        
        public ReactiveList<Instance> InstancesList { get; set; } = new ReactiveList<Instance>();
        public ReactiveList<Instance> InstancesFiltered { get; set; } = new ReactiveList<Instance>();

        public ReactiveList<Metric> MetricsList { get; set; } = new ReactiveList<Metric>();
        public ReactiveList<Metric> MetricsFiltered { get; set; } = new ReactiveList<Metric>();

        private void Refresh()
        {
            var items =
                _cache.Sources
                    .Select(s => new NodeSource
                    {
                        Value = s,
                        Instances = new ReactiveList<NodeInstance>(
                            _cache.Instances
                                .Where(ins => ins.Source.ID == s.ID)
                                .Select(ins =>
                                    new NodeInstance
                                    {
                                        Value = ins,
                                        Metrics = new ReactiveList<NodeMetric>(
                                            _cache.Metrics
                                                .Where(m => m.Instance.ID == ins.ID)
                                                .Select(m => new NodeMetric
                                                {
                                                    Value = m
                                                })
                                                .OrderBy(m => m.Value.Name)
                                        )
                                    })
                                .OrderBy(ins => ins.Value.Name)
                        )
                    })
                    .OrderBy(s => s.Value.Name);


            SourcesTree.Initialize(items);

            SourcesList.Initialize(_cache.Sources);
            SourcesFiltered.Initialize(SourcesList);

            InstancesList.Initialize(_cache.Instances);
            InstancesFiltered.Initialize(InstancesList);

            MetricsList.Initialize(_cache.Metrics);
            MetricsFiltered.Initialize(MetricsList);
        }

        private void Filter(string filter)
        {
            _filter = filter.ToLower();
            UpdateFilteredEntities();
        }

        private bool FilterStr(string str)
        {
            return str.ToLower().Contains(_filter);
        }

        private bool FilterSource(Source s) { return FilterStr(s.Name); }
        private bool FilterInstance(Instance i) { return FilterStr(i.Name) || FilterSource(i.Source); }
        private bool FilterMetric(Metric m) { return FilterStr(m.Name) || FilterInstance(m.Instance); }

        private void UpdateFilteredEntities(bool uSources = true, bool uInstances = true, bool uMetrics = true)
        {
            var hasFilter = !string.IsNullOrWhiteSpace(_filter);
            if (uSources)
                SourcesFiltered.Initialize(hasFilter ? SourcesList.Where(FilterSource) : SourcesList);
            if (uInstances)
                InstancesFiltered.Initialize(hasFilter ? InstancesList.Where(FilterInstance) : InstancesList);
            if (uMetrics)
                MetricsFiltered.Initialize(hasFilter ? MetricsList.Where(FilterMetric) : MetricsList);
        }

        private void RemoveNodeSource(NodeSource v)
        {
            var removed = RemoveSource(v.Value);
            if (removed)
            {
                SourcesTree.Remove(v);
            }
        }

        private void RemoveNodeInstance(NodeInstance v)
        {
            var removed = RemoveInstance(v.Value);
            if (removed)
            {
                foreach (var source in SourcesTree)
                    source.Instances.Remove(v);
            }
        }

        private void RemoveNodeMetric(NodeMetric v)
        {
            var removed = RemoveMetric(v.Value);
            if (removed)
            {
                foreach (var source in SourcesTree)
                    foreach (var instance in source.Instances)
                        instance.Metrics.Remove(v);
            }
        }

        private bool RemoveSource(Source v)
        {
            try
            {
                _cache.RemoveSource(v);
                
                SourcesList.Remove(v);
                InstancesList.Initialize(InstancesList.Where(ins => ins.Source != v).ToList());
                MetricsList.Initialize(MetricsList.Where(m => InstancesList.Contains(m.Instance)).ToList());

                UpdateFilteredEntities();
                return true;
            }
            catch (WebException e)
            {
                ShowPopupWebException(e);
                return false;
            }
        }

        private bool RemoveInstance(Instance v)
        {
            try
            {
                _cache.RemoveInstance(v);
                
                InstancesList.Remove(v);
                MetricsList.Initialize(MetricsList.Where(m => m.Instance != v).ToList());

                UpdateFilteredEntities(false);
                return true;
            }
            catch (WebException e)
            {
                ShowPopupWebException(e);
                return false;
            }
        }

        private bool RemoveMetric(Metric v)
        {
            try
            {
                _cache.RemoveMetric(v);

                MetricsList.Remove(v);

                UpdateFilteredEntities(false, false);
                return true;
            }
            catch (WebException e)
            {
                ShowPopupWebException(e);
                return false;
            }
        }

        private void ShowPopupWebException(WebException e)
        {
            //ToDo: show notification
            //  when (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized)
            Console.WriteLine($@"Web Exceptions {e}");
        }

    }
}

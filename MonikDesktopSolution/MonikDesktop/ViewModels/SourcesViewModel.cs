using MonikDesktop.Common;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;
using MonikDesktop.ViewModels.ShowModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class SourcesViewModel : ViewModelBase
    {
        private WithSourcesShowModel _model;

        public SourcesViewModel(IShell shell)
        {
            Title  = "Sources";

            FilteredItems = new ReactiveList<SourceItem> { ChangeTrackingEnabled = true };

            FilteredItems
               .ItemChanged
               .Subscribe(x => _model.OnSourceItemChanged(x.Sender));

            shell.WhenAnyValue(x => x.SelectedView)
                .Where(v => !(v is IToolView))
                .Subscribe(v =>
                {
                    if (v is IShowView showView &&
                        showView.ShowViewModel.Model is WithSourcesShowModel model)
                    {
                        UpdateModel(model);
                        IsEnabled = true;
                    }
                    else
                        IsEnabled = false;
                });

            this.ObservableForProperty(x => x.SelectedItem)
               .Where(v => v.Value != null)
               .Subscribe(v => SelectedHack = v.Value);

            this.ObservableForProperty(x => x.FilterText)
               .Throttle(TimeSpan.FromSeconds(0.7), RxApp.MainThreadScheduler)
               .Subscribe(v => Filter(v.Value));
        }

        [Reactive] public ReactiveCommand SelectNoneCommand  { get; set; }
        [Reactive] public ReactiveCommand SelectGroupCommand { get; set; }

        public ReactiveList<SourceItem> FilteredItems { get; }
        private SourceItem[] SourceItems => _model.Cache.SourceItems;

        [Reactive] public SourceItem SelectedHack { get; set; }
        [Reactive] public SourceItem SelectedItem { get; set; }
        [Reactive] public string FilterText { get; set; }

        private void Filter(string aFilter)
        {
            aFilter = aFilter.ToLower();

            IEnumerable<SourceItem> tmp;

            if (string.IsNullOrWhiteSpace(aFilter))
                tmp = SourceItems;
            else
                tmp = SourceItems
                   .Where(x => x.SourceName.ToLower().Contains(aFilter) ||
                               x.InstanceName.ToLower().Contains(aFilter));
            
            FilteredItems.Initialize(tmp);
        }

        private void SetChecked(Func<SourceItem, bool> condition)
        {
            // using(FilteredItems.SuppressChangeNotifications())
            // - is faster, but will cause additional update
            FilteredItems.ChangeTrackingEnabled = false;
            foreach (var it in SourceItems)
                it.Checked = condition(it);
            FilteredItems.ChangeTrackingEnabled = true;
        }

        private void SelectNone()
        {
            _model.SelectedSourcesClear();
            SetChecked(_ => false);
        }

        private void SelectGroup()
        {
            if (SelectedHack == null || _model.Groups.Contains(SelectedHack.GroupID))
                return;

            _model.SelectSourcesGroup(SelectedHack.GroupID);
            SetChecked(x => x.Checked || x.GroupID == SelectedHack.GroupID);
        }

        private void FillSourcesTree()
        {
            FilterText = "";
            FilteredItems.Initialize(SourceItems);
        }

        private void SyncCheckStatuses()
        {
            SetChecked(x => _model.Groups.Contains(x.GroupID) || _model.Instances.Contains(x.InstanceID));
        }

        private void UpdateModel(WithSourcesShowModel model)
        {
            if (model == _model)
                return;

            var cacheIsChanged = _model == null || model == null || _model.Cache != model.Cache;
            if (cacheIsChanged)
            {
                if (_model != null)
                    _model.Cache.Loaded -= OnCacheLoaded;
                if (model != null)
                    model.Cache.Loaded += OnCacheLoaded;
            }

            _model = model;
            Refresh(cacheIsChanged);
        }

        private void OnCacheLoaded()
        {
            Refresh(true);
        }

        private void Refresh(bool cacheIsChanged)
        {
            SelectedItem = null;
            SelectedHack = null;

            SelectNoneCommand?.Dispose();
            SelectGroupCommand?.Dispose();

            if (cacheIsChanged)
                FillSourcesTree();

            SyncCheckStatuses();

            // Select None Command
            var canSelectNone = _model.SelectedSourcesChanged
                .Select(_ => _model.Instances.Count > 0 || _model.Groups.Count > 0);

            SelectNoneCommand = ReactiveCommand.Create(SelectNone, canSelectNone);

            // Select Group Command
            var canSelectGroup = this.WhenAny(x => x.SelectedHack, v => v.Value)
                .Merge(_model.SelectedSourcesChanged.Select(_ => SelectedHack))
                .Select(si => si != null && !_model.Groups.Contains(si.GroupID));

            SelectGroupCommand = ReactiveCommand.Create(SelectGroup, canSelectGroup);
        }
    } //end of class
}
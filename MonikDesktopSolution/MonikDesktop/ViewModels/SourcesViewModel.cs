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
    public class SourcesViewModel : ViewModelBase, ISourcesViewModel
    {
        private WithSourcesShowModel _model;

        public SourcesViewModel(IShell shell)
        {
            Title  = "Sources";

            SourceItems = new ReactiveList<SourceItem>();

            FilteredItems = new ReactiveList<SourceItem>();

            FilteredItems
               .ItemChanged
               .Subscribe(x => OnSourceChanged(x.Sender));

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

            RefreshCommand = ReactiveCommand.Create(Refresh);

            this.ObservableForProperty(x => x.FilterText)
               .Throttle(TimeSpan.FromSeconds(0.7), RxApp.MainThreadScheduler)
               .Subscribe(v => Filter(v.Value));
        }

        [Reactive] public ReactiveCommand SelectNoneCommand  { get; set; }
        [Reactive] public ReactiveCommand SelectGroupCommand { get; set; }
        public ReactiveCommand RefreshCommand     { get; set; }

        public ReactiveList<SourceItem> FilteredItems { get; }
        public ReactiveList<SourceItem> SourceItems   { get; }

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

        private void SelectNone()
        {
            _model.Groups.Clear();
            _model.Instances.Clear();

            foreach (var it in SourceItems)
                it.Checked = false;
        }

        private void SelectGroup()
        {
            if (SelectedHack == null || _model.Groups.Contains(SelectedHack.GroupID))
                return;

            foreach (var x in SourceItems)
                x.Checked = x.Checked || x.GroupID == SelectedHack.GroupID;
        }

        private void Refresh()
        {
            _model.Cache.Reload();

            FillSourcesTree();
            SyncCheckStatuses();
        }

        private void FillSourcesTree()
        {
            using (FilteredItems.SuppressChangeNotifications())
            using (SourceItems.SuppressChangeNotifications())
            {
                SourceItems.Clear();
                FilteredItems.Clear();

                SourceItems.Initialize(_model.Cache.SourceItems);

                FilterText = "";
                FilteredItems.Initialize(SourceItems);
            }
        }

        private void SyncCheckStatuses()
        {
            using (FilteredItems.SuppressChangeNotifications())
            using (SourceItems.SuppressChangeNotifications())
                foreach (var it in SourceItems)
                    it.Checked = _model.Groups.Contains(it.GroupID) || _model.Instances.Contains(it.InstanceID);
        }

        private void UpdateModel(WithSourcesShowModel model)
        {
            if (model == _model)
                return;

            var cacheIsChanged = _model == null || model == null || _model.Cache != model.Cache;
            _model = model;

            SelectedItem = null;
            SelectedHack = null;

            SelectNoneCommand = null;
            SelectGroupCommand = null;

            if (cacheIsChanged)
                FillSourcesTree();

            SyncCheckStatuses();
            
            // Select None Command
            var canSelectNone = _model.WhenAny(
                x => x.Instances.Count,
                x => x.Groups.Count,
                (ins, gr) => ins.Value > 0 || gr.Value > 0);

            SelectNoneCommand = ReactiveCommand.Create(SelectNone, canSelectNone);

            // Select Group Command
            var canSelectGroup = this.WhenAny(x => x.SelectedHack, v => v.Value)
                .Merge(SourceItems.ItemChanged.Select(v => v.Sender))
                .Select(si => si != null && !_model.Groups.Contains(si.GroupID));

            SelectGroupCommand = ReactiveCommand.Create(SelectGroup, canSelectGroup);
        }

        /// <summary>
        ///     Manage instance and group lists. Join to groups if need.
        ///     aItem have new Checked state.
        /// </summary>
        private void OnSourceChanged(SourceItem aItem)
        {
            if (aItem.Checked)
                _model.Instances.Add(aItem.InstanceID);
            else
                _model.Instances.Remove(aItem.InstanceID);

            if (!aItem.Checked && (aItem.GroupID > 0) && _model.Groups.Contains(aItem.GroupID))
            {
                var checkedItems = SourceItems
                   .Where(x => (x.GroupID == aItem.GroupID) && x.Checked)
                   .Select(x => x.InstanceID);

                _model.Instances.AddRange(checkedItems);

                _model.Groups.Remove(aItem.GroupID);
            }

            if (aItem.Checked && (aItem.GroupID > 0) && !_model.Groups.Contains(aItem.GroupID))
            {
                var allItems = SourceItems
                   .Where(x => x.GroupID == aItem.GroupID)
                   .ToList();

                var allCheckedItems = SourceItems
                   .Where(x => (x.GroupID == aItem.GroupID) && x.Checked)
                   .ToList();

                if (allItems.Count == allCheckedItems.Count)
                {
                    _model.Groups.Add(aItem.GroupID);
                    _model.Instances.RemoveAll(allItems.Select(x => x.InstanceID));
                }
            }
        }
    } //end of class
}
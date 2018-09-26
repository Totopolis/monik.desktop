using MonikDesktop.Common;
using MonikDesktop.Common.Interfaces;
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
        private readonly ISourcesCache _cache;

        private ReactiveList<short> _selectedGroups;
        private ReactiveList<int>   _selectedInstances;

        private WithSourcesShowModel _model;

        public SourcesViewModel(IShell shell, ISourcesCache aCache)
        {
            _cache = aCache;
            Title  = "Sources";

            SourceItems = new ReactiveList<SourceItem>();

            FilteredItems = new ReactiveList<SourceItem>();

            FilteredItems
               .ItemChanged
               .Subscribe(x => OnSourceChanged(x.Sender));

            FillSourcesTree();

            shell.WhenAnyValue(x => x.SelectedView)
               .Where(v => v is IShowView)
               .Subscribe(v => OnSelectedWindow(v as IShowView));

            this.ObservableForProperty(x => x.SelectedItem)
               .Where(v => v.Value != null)
               .Subscribe(v => SelectedHack = v.Value);

            RefreshCommand = ReactiveCommand.Create(Refresh);

            this.ObservableForProperty(x => x.FilterText)
               .Throttle(TimeSpan.FromSeconds(0.7), RxApp.MainThreadScheduler)
               .Subscribe(v => Filter(v.Value));
        }

        public ReactiveCommand SelectNoneCommand  { get; set; }
        public ReactiveCommand SelectGroupCommand { get; set; }
        public ReactiveCommand RefreshCommand     { get; set; }

        public ReactiveList<SourceItem> FilteredItems { get; private set; }
        public ReactiveList<SourceItem> SourceItems   { get; private set; }

        [Reactive]public SourceItem SelectedHack { get; set; }
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
            _selectedGroups.Clear();
            _selectedInstances.Clear();

            foreach (var it in SourceItems)
                it.Checked = false;
        }

        private void SelectGroup()
        {
            if (SelectedHack == null || _selectedGroups.Contains(SelectedHack.GroupID))
                return;

            foreach (var x in SourceItems.Where(x => x.GroupID == SelectedHack.GroupID).ToList())
                x.Checked = true;
        }

        private void Refresh()
        {
            _cache.Reload();

            FillSourcesTree();
            SyncCheckStatuses();

            this.RaisePropertyChanged("SourceItems");
        }

        private void FillSourcesTree()
        {
            SourceItems.Clear();
            FilteredItems.Clear();

            var groups    = _cache.Groups;
            var instances = _cache.Instances;

            var srcItems = new List<SourceItem>();
            var _sourceItems = groups.SelectMany(gr => gr.Instances,
                                                 (gr, inst) => new SourceItem
                                                 {
                                                     GroupID      = gr.ID,
                                                     GroupName    = gr.Name,
                                                     SourceName   = inst.Source.Name,
                                                     InstanceName = inst.Name,
                                                     InstanceID   = inst.ID
                                                 });

            srcItems.AddRange(_sourceItems);

            _sourceItems = instances.Where(inst => srcItems.All(x => x.InstanceID != inst.ID))
               .Select(inst => new SourceItem
                {
                    GroupID      = 0,
                    GroupName    = "[NOGROUP]",
                    SourceName   = inst.Source.Name,
                    InstanceName = inst.Name,
                    InstanceID   = inst.ID
                });

            srcItems.AddRange(_sourceItems);

            SourceItems.Initialize(srcItems);

            FilterText = "";
            FilteredItems.ChangeTrackingEnabled = false;
            FilteredItems.Initialize(srcItems);
            FilteredItems.ChangeTrackingEnabled = true;
        }

        private void SyncCheckStatuses()
        {
            SourceItems.ChangeTrackingEnabled = false;

            foreach (var it in SourceItems)
                it.Checked = false;

            // fill from IShowWindow
            foreach (var it in SourceItems.Where(it => _selectedGroups.Contains(it.GroupID) || _selectedInstances.Contains(it.InstanceID)).ToList())
                it.Checked = true;

            SourceItems.ChangeTrackingEnabled = true;
        }

        private void OnSelectedWindow(IShowView aWindow)
        {
            if (aWindow.ShowViewModel.Model is WithSourcesShowModel model)
            {
                IsEnabled = true;

                if (model == _model)
                    return;

                _model = model;

                SelectNoneCommand = null;
                SelectGroupCommand = null;

                _selectedGroups = _model.Groups;
                _selectedInstances = _model.Instances;

                SyncCheckStatuses();

                // update view
                this.RaisePropertyChanged("SourceItems");

                // Select None Command
                var canSelectNone = _model.WhenAny(
                    x => x.Instances.Count,
                    x => x.Groups.Count,
                    (ins, gr) => ins.Value > 0 || gr.Value > 0);

                SelectNoneCommand = ReactiveCommand.Create(SelectNone, canSelectNone);

                // Select Group Command
                var canSelectGroup = this.WhenAny(x => x.SelectedHack, v => v.Value)
                    .Merge(SourceItems.ItemChanged.Select(v => v.Sender))
                    .Select(si => si != null && !_selectedGroups.Contains(si.GroupID));

                SelectGroupCommand = ReactiveCommand.Create(SelectGroup, canSelectGroup);
            }
            else
            {
                IsEnabled = false;
            }
        }

        /// <summary>
        ///     Manage instance and group lists. Join to groups if need.
        ///     aItem have new Checked state.
        /// </summary>
        private void OnSourceChanged(SourceItem aItem)
        {
            if (aItem.Checked)
                _selectedInstances.Add(aItem.InstanceID);
            else
                _selectedInstances.Remove(aItem.InstanceID);

            if (!aItem.Checked && (aItem.GroupID > 0) && _selectedGroups.Contains(aItem.GroupID))
            {
                var checkedItems = SourceItems
                   .Where(x => (x.GroupID == aItem.GroupID) && x.Checked)
                   .Select(x => x.InstanceID);

                _selectedInstances.AddRange(checkedItems);

                _selectedGroups.Remove(aItem.GroupID);
            }

            if (aItem.Checked && (aItem.GroupID > 0) && !_selectedGroups.Contains(aItem.GroupID))
            {
                var allItems = SourceItems
                   .Where(x => x.GroupID == aItem.GroupID)
                   .ToList();

                var allCheckedItems = SourceItems
                   .Where(x => (x.GroupID == aItem.GroupID) && x.Checked)
                   .ToList();

                if (allItems.Count == allCheckedItems.Count)
                {
                    _selectedGroups.Add(aItem.GroupID);
                    _selectedInstances.RemoveAll(allItems.Select(x => x.InstanceID));
                }
            }
        }
    } //end of class
}
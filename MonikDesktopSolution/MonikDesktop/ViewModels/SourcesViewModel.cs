using DynamicData;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;
using MonikDesktop.ViewModels.ShowModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;
using MonikDesktop.Common;
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

            var dynamicFilter = this.ObservableForProperty(x => x.FilterText)
               .Throttle(TimeSpan.FromSeconds(0.7), RxApp.MainThreadScheduler)
               .Select(v => Filters.CreateFilterSourceItem(v.Value));

            _model.Cache.SourceItems
                .Connect()
                .Filter(dynamicFilter);
        }

        [Reactive] public ReactiveCommand SelectNoneCommand  { get; set; }
        [Reactive] public ReactiveCommand SelectGroupCommand { get; set; }

        public ReactiveList<SourceItem> FilteredItems { get; }
        
        [Reactive] public SourceItem SelectedHack { get; set; }
        [Reactive] public SourceItem SelectedItem { get; set; }
        [Reactive] public string FilterText { get; set; }

        private void SetChecked(Func<SourceItem, bool> condition)
        {
            _model.Cache.SourceItems.Edit(innerCache =>
            {
                foreach (var it in innerCache.Items)
                    it.Checked = condition(it);
            });
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
            _model = model;
            Refresh(cacheIsChanged);
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
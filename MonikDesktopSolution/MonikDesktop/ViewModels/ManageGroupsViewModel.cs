using DynamicData;
using DynamicData.Binding;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class ManageGroupsViewModel : ViewModelBase
    {
        private readonly ISourcesCache _cache;
        private readonly IDockWindow _window;

        public ManageGroupsViewModel(ISourcesCacheProvider cacheProvider, IDockWindow window)
        {
            _cache = cacheProvider.CurrentCache;
            _window = window;

            Title = "Manage Groups";

            _cache.Groups
                .Connect()
                .Sort(SortExpressionComparer<Group>
                    .Ascending(x => x.ID))
                .Bind(out _listGroups)
                .Subscribe()
                .DisposeWith(Disposables);

            _cache.InstancesWithoutGroup
                .Connect()
                .Sort(SortExpressionComparer<Instance>
                    .Ascending(x => x.ID))
                .Bind(out _listWithoutGroup)
                .Subscribe()
                .DisposeWith(Disposables);

            var dynamicInListFilter = this
                .WhenAnyValue(x => x.SelectedGroup)
                .Select(g => (Func<Group, bool>) (x => g != null && x.ID == g.ID));

            _cache.Groups
                .Connect()
                .Filter(dynamicInListFilter)
                .TransformMany(x => x.Instances, x => x.ID)
                .Sort(SortExpressionComparer<Instance>
                    .Ascending(x => x.ID))
                .Bind(out _listInGroup)
                .Subscribe()
                .DisposeWith(Disposables);

            SelectedGroup = ListGroups.FirstOrDefault();

            RemoveGroupCommand = ReactiveCommand.Create<Group>(RemoveGroup);
            CreateGroupCommand = ReactiveCommand.Create(CreateGroup);
        }

        [Reactive] public Group SelectedGroup { get; set; }
        private readonly ReadOnlyObservableCollection<Group> _listGroups;
        public ReadOnlyObservableCollection<Group> ListGroups => _listGroups;
        private readonly ReadOnlyObservableCollection<Instance> _listWithoutGroup;
        public ReadOnlyObservableCollection<Instance> ListWithoutGroup => _listWithoutGroup;
        private readonly ReadOnlyObservableCollection<Instance> _listInGroup;
        public ReadOnlyObservableCollection<Instance> ListInGroup => _listInGroup;

        public ReactiveCommand RemoveGroupCommand { get; set; }
        public ReactiveCommand CreateGroupCommand { get; set; }

        private void RemoveGroup(Group gItem)
        {
            try
            {
                _cache.RemoveGroup(gItem);
                if (SelectedGroup == gItem)
                    SelectedGroup = ListGroups.FirstOrDefault();
            }
            catch (WebException e)
            {
                ShowPopupWebException(e);
            }
        }

        private async void CreateGroup()
        {
            var result = await ((MainWindow)_window).ShowGroupCreateDialog();

            if (result == null)
                return;

            try
            {
                var newGroup = _cache.CreateGroup(result.Name, result.IsDeafult, result.Description);
                SelectedGroup = newGroup;
            }
            catch (WebException e)
            {
                ShowPopupWebException(e);
            }
        }

        public void AddInstanceToCurrentGroup(Instance instance)
        {
            if (SelectedGroup == null)
                return;

            try
            {
                _cache.AddInstanceToGroup(instance, SelectedGroup);
            }
            catch (WebException e)
            {
                ShowPopupWebException(e);
            }
        }

        public void RemoveInstanceFromCurrentGroup(Instance instance)
        {
            if (SelectedGroup == null)
                return;

            try
            {
                _cache.RemoveInstanceFromGroup(instance, SelectedGroup);
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

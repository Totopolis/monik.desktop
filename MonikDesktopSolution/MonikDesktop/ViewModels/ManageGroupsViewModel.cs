using DynamicData;
using MonikDesktop.Common;
using MonikDesktop.Common.Interfaces;
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
using Instance = MonikDesktop.Common.ModelsApp.Instance;

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
                .Transform(x => new GroupItem(x))
                .Bind(out _listGroups)
                .Subscribe()
                .DisposeWith(Disposables);

            _cache.InstancesWithoutGroup
                .Connect()
                .Bind(out _listWithoutGroup)
                .Subscribe()
                .DisposeWith(Disposables);

            var dynamicInListFilter = this
                .WhenAnyValue(x => x.SelectedGroup)
                .Select(Filters.CreateFilterSelectedGroup);

            _cache.Groups
                .Connect()
                .Filter(dynamicInListFilter)
                .TransformMany(x => x.Instances, x => x.ID)
                .Bind(out _listInGroup)
                .Subscribe()
                .DisposeWith(Disposables);

            SelectedGroup = ListGroups.FirstOrDefault();

            RemoveGroupCommand = ReactiveCommand.Create<GroupItem>(RemoveGroup);
            CreateGroupCommand = ReactiveCommand.Create(CreateGroup);
        }

        [Reactive] public GroupItem SelectedGroup { get; set; }
        private readonly ReadOnlyObservableCollection<GroupItem> _listGroups;
        public ReadOnlyObservableCollection<GroupItem> ListGroups => _listGroups;
        private readonly ReadOnlyObservableCollection<Instance> _listWithoutGroup;
        public ReadOnlyObservableCollection<Instance> ListWithoutGroup => _listWithoutGroup;
        private readonly ReadOnlyObservableCollection<Instance> _listInGroup;
        public ReadOnlyObservableCollection<Instance> ListInGroup => _listInGroup;

        public ReactiveCommand RemoveGroupCommand { get; set; }
        public ReactiveCommand CreateGroupCommand { get; set; }

        private void RemoveGroup(GroupItem gItem)
        {
            try
            {
                _cache.RemoveGroup(gItem.Group);

                ListWithoutGroup.AddRange(gItem.Group.Instances);

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
                var group = _cache.CreateGroup(result.Name, result.IsDeafult, result.Description);

                // ToDo: Select created group in view
                //SelectedGroup = gItem;
            }
            catch (WebException e)
            {
                ShowPopupWebException(e);
            }
        }

        public void AddInstanceToCurrentGroup(Instance instance)
        {
            try
            {
                _cache.AddInstanceToGroup(instance, SelectedGroup.Group);
            }
            catch (WebException e)
            {
                ShowPopupWebException(e);
            }
        }

        public void RemoveInstanceFromCurrentGroup(Instance instance)
        {
            try
            {
                _cache.RemoveInstanceFromGroup(instance, SelectedGroup.Group);
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

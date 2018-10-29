using MonikDesktop.Common;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Net;
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

            ListGroups = new ReactiveList<GroupItem>();
            ListInGroup = new ReactiveList<Instance>();
            ListWithoutGroup = new ReactiveList<Instance>();

            this.ObservableForProperty(x => x.SelectedGroup)
                .Subscribe(v => ShowGroup(v.Value));

            _cache.Loaded += Refresh;
            Refresh();

            RemoveGroupCommand = ReactiveCommand.Create<GroupItem>(RemoveGroup);
            CreateGroupCommand = ReactiveCommand.Create(CreateGroup);
        }

        [Reactive] public GroupItem SelectedGroup { get; set; }
        public ReactiveList<GroupItem> ListGroups { get; set; }
        public ReactiveList<Instance> ListInGroup { get; set; }
        public ReactiveList<Instance> ListWithoutGroup { get; set; }

        public ReactiveCommand RemoveGroupCommand { get; set; }
        public ReactiveCommand CreateGroupCommand { get; set; }

        private void Refresh()
        {
            ListGroups.Initialize(_cache.Groups.Select(x => new GroupItem(x)));
            ListWithoutGroup.Initialize(_cache.Instances.Where(ins => _cache.Groups.All(g => !g.Instances.Contains(ins))));

            SelectedGroup = ListGroups.FirstOrDefault();
        }

        private void ShowGroup(GroupItem gItem)
        {
            if (gItem == null)
                ListInGroup.Clear();
            else
                ListInGroup.Initialize(gItem.Group.Instances);
        }

        private void RemoveGroup(GroupItem gItem)
        {
            try
            {
                _cache.RemoveGroup(gItem.Group);

                ListGroups.Remove(gItem);
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

                var gItem = new GroupItem(group);
                ListGroups.Add(gItem);
                SelectedGroup = gItem;
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
                SelectedGroup.UpdateAmount();

                ListWithoutGroup.Remove(instance);
                ListInGroup.Add(instance);
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
                SelectedGroup.UpdateAmount();

                ListInGroup.Remove(instance);
                ListWithoutGroup.Add(instance);
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

        protected override void DisposeInternals()
        {
            base.DisposeInternals();
            _cache.Loaded -= Refresh;
        }
    }
}

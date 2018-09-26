using MonikDesktop.Common;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Net;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class ManageGroupsViewModel : ViewModelBase, IManageGroupsViewModel
    {
        private readonly ISourcesCache _cache;

        public ManageGroupsViewModel(ISourcesCache cache)
        {
            _cache = cache;

            Title = "Manage Groups";

            ListGroups = new ReactiveList<GroupItem>(_cache.Groups.Select(x => new GroupItem(x)));
            ListInGroup = _cache.Groups.Length > 0 ? new ReactiveList<Instance>(_cache.Groups[0].Instances) : new ReactiveList<Instance>();
            ListWithoutGroup = new ReactiveList<Instance>(_cache.Instances.Where(ins => _cache.Groups.All(g => !g.Instances.Contains(ins))));

            SelectedGroup = ListGroups.FirstOrDefault();
            this.ObservableForProperty(x => x.SelectedGroup)
                .Subscribe(v => ShowGroup(v.Value));

            RemoveGroupCommand = ReactiveCommand.Create<GroupItem>(RemoveGroup);
        }

        [Reactive] public GroupItem SelectedGroup { get; set; }
        public ReactiveList<GroupItem> ListGroups { get; set; }
        public ReactiveList<Instance> ListInGroup { get; set; }
        public ReactiveList<Instance> ListWithoutGroup { get; set; }

        public ReactiveCommand RemoveGroupCommand { get; set; }

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
            //ToDo: show notification
            Console.WriteLine($@"Web Exception {e}");
        }
        
    }
}

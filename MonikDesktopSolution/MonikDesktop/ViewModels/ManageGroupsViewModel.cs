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

            ListGroups = new ReactiveList<Group>(_cache.Groups);
            ListInGroup = _cache.Groups.Length > 0 ? new ReactiveList<Instance>(_cache.Groups[0].Instances) : new ReactiveList<Instance>();
            ListWithoutGroup = new ReactiveList<Instance>(_cache.Instances.Where(ins => _cache.Groups.All(g => !g.Instances.Contains(ins))));

            SelectedGroup = _cache.Groups.Length > 0 ? _cache.Groups[0] : null;
            this.ObservableForProperty(x => x.SelectedGroup)
                .Subscribe(v => ShowGroup(v.Value));

            RemoveGroupCommand = ReactiveCommand.Create<Group>(RemoveGroup);
        }

        [Reactive] public Group SelectedGroup { get; set; }
        public ReactiveList<Group> ListGroups { get; set; }
        public ReactiveList<Instance> ListInGroup { get; set; }
        public ReactiveList<Instance> ListWithoutGroup { get; set; }

        public ReactiveCommand RemoveGroupCommand { get; set; }

        private void ShowGroup(Group g)
        {
            ListInGroup.Initialize(g.Instances);
        }

        private void RemoveGroup(Group g)
        {
            try
            {
                _cache.RemoveGroup(g);

                ListGroups.Remove(g);
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
                _cache.AddInstanceToGroup(instance, SelectedGroup);

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
                _cache.RemoveInstanceFromGroup(instance, SelectedGroup);
                
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

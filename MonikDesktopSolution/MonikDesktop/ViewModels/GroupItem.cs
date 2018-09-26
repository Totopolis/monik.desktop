using MonikDesktop.Common.ModelsApp;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop.ViewModels
{
    public class GroupItem : ReactiveObject
    {
        public Group Group { get; set; }
        [Reactive] public int InstancesAmount { get; set; }

        public GroupItem(Group g)
        {
            Group = g;
            UpdateAmount();
        }

        public void UpdateAmount()
        {
            InstancesAmount = Group.Instances.Count;
        }
    }
}
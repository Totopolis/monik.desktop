using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;
using ReactiveUI;
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
            ListWithoutGroup = new ReactiveList<Instance>(); //new ReactiveList<Instance>(_cache.Instances.Where(ins => _cache.Groups.All(g => !g.Instances.Contains(ins))));

            var rnd = new System.Random();
            TempAdd = ReactiveCommand.Create(() =>
            {
                ListWithoutGroup.Add(
                    new Instance {ID = rnd.Next(), Name = "HoHo", Source = new Source {ID = (short)rnd.Next(), Name = "HaHa"}});
            });
            TempSub = ReactiveCommand.Create(() =>
            {
                if (ListWithoutGroup.Count > 0)
                    ListWithoutGroup.RemoveAt(0);
            });
        }

        public ReactiveCommand TempAdd { get; set; }
        public ReactiveCommand TempSub { get; set; }

        public ReactiveList<Group> ListGroups { get; set; }
        public ReactiveList<Instance> ListInGroup { get; set; }
        public ReactiveList<Instance> ListWithoutGroup { get; set; }

        public void AddInstanceToCurrentGroup(Instance instance)
        {
            ListWithoutGroup.Remove(instance);
            ListInGroup.Add(instance);
        }

        public void RemoveInstanceFromCurrentGroup(Instance instance)
        {
            ListInGroup.Remove(instance);
            ListWithoutGroup.Add(instance);
        }
    }
}

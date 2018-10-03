using MonikDesktop.Common.ModelsApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MonikDesktop.ViewModels.ShowModels
{
    public class WithSourcesShowModel : ShowModel
    {
        private readonly Subject<Unit> _selectedSourcesChanged;
        public IObservable<Unit> SelectedSourcesChanged => _selectedSourcesChanged.AsObservable();
        public HashSet<short> Groups { get; }
        public HashSet<int> Instances { get; }

        public WithSourcesShowModel(bool disableWhenSourcesChanged = true)
        {
            _selectedSourcesChanged = new Subject<Unit>();

            Groups = new HashSet<short>();
            Instances = new HashSet<int>();

            if (disableWhenSourcesChanged)
                SelectedSourcesChanged.Subscribe(_ => Online = false);
        }

        public void SelectedSourcesClear()
        {
            Groups.Clear();
            Instances.Clear();

            _selectedSourcesChanged.OnNext(Unit.Default);
        }

        public void SelectSourcesGroup(short groupId)
        {
            Groups.Add(groupId);
            var gInstances = Cache.GetGroup(groupId).Instances;
            foreach (var i in gInstances)
                Instances.Remove(i.ID);

            _selectedSourcesChanged.OnNext(Unit.Default);
        }

        protected override void OnCacheLoaded()
        {
            base.OnCacheLoaded();
            SelectedSourcesClear();
        }

        /// <summary>
        ///     Manage instance and group lists. Join to groups if needed.
        /// </summary>
        public void OnSourceItemChanged(SourceItem item)
        {
            if (item.Checked)
                Instances.Add(item.InstanceID);
            else
                Instances.Remove(item.InstanceID);

            if (!item.Checked && item.GroupID > 0 && Groups.Contains(item.GroupID))
            {
                var checkedItems = Cache.SourceItems
                    .Where(x => x.GroupID == item.GroupID && x.Checked)
                    .Select(x => x.InstanceID);

                Groups.Remove(item.GroupID);
                Instances.UnionWith(checkedItems);
            }

            if (item.Checked && item.GroupID > 0 && !Groups.Contains(item.GroupID))
            {
                var itemsInGroup = Cache.GetGroup(item.GroupID).Instances;

                var checkedItems = Cache.SourceItems
                    .Where(x => (x.GroupID == item.GroupID) && x.Checked)
                    .ToList();

                if (itemsInGroup.Count == checkedItems.Count)
                {
                    Groups.Add(item.GroupID);
                    Instances.ExceptWith(itemsInGroup.Select(x => x.ID));
                }
            }

            _selectedSourcesChanged.OnNext(Unit.Default);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _selectedSourcesChanged.Dispose();
            }
        }
    }
}

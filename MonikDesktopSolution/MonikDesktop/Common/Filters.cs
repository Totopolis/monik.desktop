using MonikDesktop.Common.ModelsApp;
using System;
using System.Linq;
using MonikDesktop.ViewModels;
using MonikDesktop.ViewModels.ShowModels;

namespace MonikDesktop.Common
{
    public static class Filters
    {
        public static Func<Source, bool> CreateFilterSource(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return _ => true;

            filter = filter.ToLower();
            return s => Check(s.Name, filter);
        }

        public static Func<Instance, bool> CreateFilterInstance(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return _ => true;

            filter = filter.ToLower();
            return s => Check(s.Name, filter) ||
                        Check(s.Source.Name, filter);
        }

        public static Func<Metric, bool> CreateFilterMetric(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return _ => true;

            filter = filter.ToLower();
            return s => Check(s.Name, filter) ||
                        Check(s.Instance.Name, filter) ||
                        Check(s.Instance.Source.Name, filter);
        }

        public static Func<SourceItem, bool> CreateFilterSourceItem(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return _ => true;

            filter = filter.ToLower();
            return si => Check(si.SourceName, filter) ||
                         Check(si.InstanceName, filter);
        }

        private static bool Check(string s, string filter)
        {
            return !string.IsNullOrEmpty(s) &&
                   s.ToLower().Contains(filter);
        }

        public static Func<Metric, bool> CreateFilterMetricBySources(WithSourcesShowModel model)
        {
            if (model.Instances.Count == 0 && model.Groups.Count == 0)
                return _ => true;

            return metric =>
            {
                if (model.Instances.Contains(metric.Instance.ID))
                    return true;

                var groupId = model.Cache.Groups.Items.FirstOrDefault(x => x.Instances.Contains(metric.Instance))?.ID;
                return groupId.HasValue && model.Groups.Contains(groupId.Value);
            };
        }

        public static Func<Group, bool> CreateFilterSelectedGroup(GroupItem gi)
        {
            if (gi == null)
                return _ => false;

            return g => g.ID == gi.Group.ID;
        }
    }
}

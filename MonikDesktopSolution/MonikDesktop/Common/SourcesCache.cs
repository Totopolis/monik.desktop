using DynamicData;
using DynamicData.Kernel;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApi;
using MonikDesktop.Common.ModelsApp;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonikDesktop.Common
{
    public class SourcesCache : ISourcesCache
    {
        private readonly IMonikService _service;

        public SourceCache<Group, short> Groups { get; set; }
        public SourceCache<Source, short> Sources { get; set; }
        public SourceCache<Metric, int> Metrics { get; set; }
        public SourceCache<Instance, int> Instances { get; set; }
        public SourceCache<Instance, int> InstancesWithoutGroup { get; set; }
        public SourceCache<SourceItem, int> SourceItems { get; set; }

        private readonly Source _unknownSource;
		private readonly Instance _unknownInstance;

		public SourcesCache(Uri url, Func<Uri, IMonikService> serviceFactory)
		{
			_service = serviceFactory(url);

            Groups = new SourceCache<Group, short>(x => x.ID);
		    Sources = new SourceCache<Source, short>(x => x.ID);
		    Metrics = new SourceCache<Metric, int>(x => x.ID);
		    Instances = new SourceCache<Instance, int>(x => x.ID);
		    InstancesWithoutGroup = new SourceCache<Instance, int>(x => x.ID);
		    SourceItems = new SourceCache<SourceItem, int>(x => x.InstanceID);

		    _unknownInstance = new Instance
		    {
		        ID = -1,
		        Name = "_UNKNOWN_",
		        Metrics = new ObservableCollection<Metric>()
		    };
            _unknownSource = new Source
		    {
		        ID = -1,
		        Name = "_UNKNOWN_",
		        Instances = new ObservableCollection<Instance>(new [] {_unknownInstance})
		    };
		    _unknownInstance.Source = _unknownSource;
		}

        public bool IsLoaded { get; set; }
	    public async Task Load()
	    {
	        await Task.Run(() => LoadCurrentState());
        }

        public IMonikService Service => _service;

        private void LoadCurrentState()
		{
            var sources = _service.GetSources();
		    var instances = _service.GetInstances();
		    var metrics = _service.GetMetrics();
            var groups = _service.GetGroups();


		    Sources.Edit(innerCache =>
		    {
		        var items = sources.Select(x => new Source
		        {
		            ID = x.ID,
		            Name = x.Name,
                    Instances = new ObservableCollection<Instance>()
		        });

		        innerCache.Clear();
		        innerCache.AddOrUpdate(items);
		        innerCache.AddOrUpdate(_unknownSource);
            });

		    Instances.Edit(innerCache =>
		    {
		        var items = instances.Select(x =>
		        {
		            var source = Sources.Lookup((short) x.SourceID).ValueOr(() => _unknownSource);
		            var instance = new Instance
		            {
		                ID = x.ID,
		                Name = x.Name,
		                Source = source,
                        Metrics = new ObservableCollection<Metric>()
		            };
                    source.Instances.Add(instance);
		            return instance;
		        });

		        innerCache.Clear();
		        innerCache.AddOrUpdate(items);
                innerCache.AddOrUpdate(_unknownInstance);
		    });

		    InstancesWithoutGroup.Edit(innerCache =>
		    {
		        var idsInGroup = groups
		            .SelectMany(x => x.Instances)
		            .Distinct();
		        var idsWithoutGroup = instances
		            .Select(x => x.ID)
		            .Except(idsInGroup);

		        var items = idsWithoutGroup.Select(id => Instances.Lookup(id).Value);

                innerCache.Clear();
                innerCache.AddOrUpdate(items);
            });

		    Metrics.Edit(innerCache =>
		    {
		        var items = metrics.Select(x =>
		        {
		            var instance = Instances.Lookup(x.InstanceID).ValueOr(() => _unknownInstance);
                    var metric = new Metric
		            {
		                ID = x.ID,
		                Name = x.Name,
		                Instance = instance,
		                Aggregation = x.Aggregation
		            };
                    instance.Metrics.Add(metric);
                    return metric;
		        });

		        innerCache.Clear();
                innerCache.AddOrUpdate(items);
		    });

		    Groups.Edit(innerCache =>
		    {
		        var items = groups.Select(x => new Group
		        {
		            ID = x.ID,
		            IsDefault = x.IsDefault,
		            Name = x.Name,
		            Instances = new ObservableCollection<Instance>(x.Instances
		                .Select(v => Instances.Lookup(v).ValueOrDefault())
		                .Where(v => v != null))
		        });

		        innerCache.Clear();
                innerCache.AddOrUpdate(items);
		    });

            SourceItems.Edit(innerCache =>
            {
                var itemsInGroup = Groups.Items.SelectMany(
                    x => x.Instances,
                    (g, i) => new SourceItem
                    {
                        GroupID = g.ID,
                        GroupName = g.Name,
                        SourceName = i.Source.Name,
                        InstanceName = i.Name,
                        InstanceID = i.ID
                    }).ToList();
                var itemsWithoutGroup = InstancesWithoutGroup.Items.Select(i => new SourceItem
                {
                    GroupID = 0,
                    GroupName = "[NOGROUP]",
                    SourceName = i.Source.Name,
                    InstanceName = i.Name,
                    InstanceID = i.ID
                });

                innerCache.Clear();
                innerCache.AddOrUpdate(itemsInGroup);
                innerCache.AddOrUpdate(itemsWithoutGroup);
            });
		}

	    public void RemoveSource(Source v)
	    {
	        if (v == _unknownSource)
	        {
	            foreach (var instance in v.Instances)
	                RemoveInstance(instance);

	            return;
	        }

            _service.RemoveSource(v.ID);
	        
	        Sources.Remove(v);
            foreach(var ins in Instances.Items)
	            if (ins.Source == v)
	                RemoveInstanceFromCache(ins);
	    }

	    public void RemoveInstance(Instance v)
	    {
	        if (v == _unknownInstance)
	        {
	            foreach (var metric in v.Metrics)
	                RemoveMetric(metric);

	            return;
	        }

	        _service.RemoveInstance(v.ID);
	        RemoveInstanceFromCache(v);
	    }

	    public void RemoveMetric(Metric v)
	    {
	        _service.RemoveMetric(v.ID);
	        Metrics.Remove(v);
	        v.Instance.Metrics.Remove(v);
	    }

        public Group GetGroup(short groupId)
        {
            return Groups.Lookup(groupId).ValueOrDefault();
        }

	    public Instance GetInstance(int aInstanceId)
		{
			return Instances.Lookup(aInstanceId).ValueOr(() => _unknownInstance);
		}

	    public void AddInstanceToGroup(Instance i, Group g)
	    {
	        _service.AddInstanceToGroup(i.ID, g.ID);
            g.Instances.Add(i);
            InstancesWithoutGroup.Remove(i);
	    }

	    public void RemoveInstanceFromGroup(Instance i, Group g)
	    {
	        _service.RemoveInstanceFromGroup(i.ID, g.ID);
	        g.Instances.Remove(i);
            InstancesWithoutGroup.AddOrUpdate(i);
        }

	    public Group CreateGroup(string name, bool isDefault, string description)
	    {
	        var newGroup = _service.CreateGroup(new EGroupCreateRequest
	        {
	            Name = name,
	            IsDefault = isDefault,
	            Description = description
	        });

	        var gr = new Group
	        {
	            ID = newGroup.ID,
	            IsDefault = newGroup.IsDefault,
	            Name = newGroup.Name,
	            Instances = new ObservableCollection<Instance>()
	        };

            Groups.AddOrUpdate(gr);
	        return gr;
	    }

        public void RemoveGroup(Group g)
	    {
	        _service.RemoveGroup(g.ID);
	        Groups.Remove(g);
	        InstancesWithoutGroup.AddOrUpdate(g.Instances);
        }


        private void RemoveInstanceFromCache(Instance ins)
        {
            // remove from parent
            ins.Source.Instances.Remove(ins);
            // remove from instances
	        Instances.Remove(ins.ID);
            // cleanup groups
            Groups.Edit(innerCache =>
            {
                foreach (var g in innerCache.Items)
                {
                    var removed = g.Instances.Remove(ins);
                    if (removed)
                        innerCache.Refresh(g);
                }
            });
            // claenup metrics
	        Metrics.Remove(Metrics.Items.Where(m => m.Instance == ins));
        }

	} //end of class
}
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApi;
using MonikDesktop.Common.ModelsApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonikDesktop.Common
{
    public class SourcesCache : ISourcesCache
    {
        private readonly IMonikService _service;

        private SourcesCacheState _state;


        private readonly Source _unknownSource;
		private readonly Instance _unknownInstance;

		public SourcesCache(Uri url, Func<Uri, IMonikService> serviceFactory)
		{
			_service = serviceFactory(url);

			_unknownSource = new Source {ID = -1, Name = "_UNKNOWN_"};
			_unknownInstance = new Instance {ID = -1, Name = "_UNKNOWN_", Source = _unknownSource};
		}

        public event Action Loaded;
        public bool IsLoaded { get; set; }
	    public async Task Load()
	    {
	        _state = await Task.Run((Func<SourcesCacheState>)LoadCurrentState);
            Loaded?.Invoke();
        }

        public IMonikService Service => _service;

        private SourcesCacheState LoadCurrentState()
		{
            var state = new SourcesCacheState();

            var sources = _service.GetSources();
		    var instances = _service.GetInstances();
		    var metrics = _service.GetMetrics();
            var groups = _service.GetGroups();


			state.Sources = sources.Select(x => new Source
			{
				ID = x.ID,
				Name = x.Name
			}).ToList();

		    state.Instances = instances.ToDictionary(
		        it => it.ID,
		        it => new Instance
		        {
		            ID = it.ID,
		            Name = it.Name,
		            Source = state.Sources.FirstOrDefault(x => x.ID == it.SourceID) ?? _unknownSource
		        });

		    state.Metrics = metrics.Select(m =>
		        new Metric
		        {
		            ID = m.ID,
		            Name = m.Name,
		            Instance = state.Instances.ContainsKey(m.InstanceID) ? state.Instances[m.InstanceID] : _unknownInstance,
		            Aggregation = m.Aggregation
		        }
		    ).ToList();

            var instancesInGroup = new HashSet<int>();

		    state.Groups = groups.Select(it =>
		    {
		        var gr = new Group
		        {
		            ID = it.ID,
		            IsDefault = it.IsDefault,
		            Name = it.Name,
		            Instances = it.Instances
		                .Where(v => state.Instances.ContainsKey(v))
		                .Select(v => state.Instances[v])
		                .ToList()
		        };

		        instancesInGroup.UnionWith(it.Instances);

		        return gr;
		    }).ToList();

		    state.SourceItems = state.Groups
		        .SelectMany(gr => gr.Instances,
		            (gr, inst) => new SourceItem
		            {
		                GroupID = gr.ID,
		                GroupName = gr.Name,
		                SourceName = inst.Source.Name,
		                InstanceName = inst.Name,
		                InstanceID = inst.ID
		            })
		        .Concat(state.Instances.Values
		            .Where(inst => !instancesInGroup.Contains(inst.ID))
		            .Select(inst => new SourceItem
		            {
		                GroupID = 0,
		                GroupName = "[NOGROUP]",
		                SourceName = inst.Source.Name,
		                InstanceName = inst.Name,
		                InstanceID = inst.ID
		            }))
		        .ToList();

		    return state;
		}

	    public Group[] Groups => _state.Groups.ToArray();

		public Source[] Sources => _state.Sources.ToArray();

		public Instance[] Instances => _state.Instances.Values.ToArray();

	    public Metric[] Metrics => _state.Metrics.ToArray();

        public SourceItem[] SourceItems => _state.SourceItems.ToArray();

	    public void RemoveSource(Source v)
	    {
	        _service.RemoveSource(v.ID);
	        
	        _state.Sources.Remove(v);
            foreach(var ins in _state.Instances.Values.ToArray())
	            if (ins.Source == v)
	                RemoveInstanceFromCache(ins);
	    }

	    public void RemoveInstance(Instance v)
	    {
	        _service.RemoveInstance(v.ID);
	        RemoveInstanceFromCache(v);
	    }

	    public void RemoveMetric(Metric v)
	    {
	        _service.RemoveMetric(v.ID);
	        _state.Metrics.Remove(v);
	    }

        public Group GetGroup(short groupId)
        {
            return _state.Groups.FirstOrDefault(g => g.ID == groupId);
        }

	    public Instance GetInstance(int aInstanceId)
		{
			return _state.Instances.ContainsKey(aInstanceId)
				? _state.Instances[aInstanceId]
				: _unknownInstance;

			// TODO: if unknown instance then update from api?
		}

	    public void AddInstanceToGroup(Instance i, Group g)
	    {
	        _service.AddInstanceToGroup(i.ID, g.ID);
            g.Instances.Add(i);
	    }

	    public void RemoveInstanceFromGroup(Instance i, Group g)
	    {
	        _service.RemoveInstanceFromGroup(i.ID, g.ID);
	        g.Instances.Remove(i);
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
	            Instances = new List<Instance>()
	        };

            _state.Groups.Add(gr);
	        return gr;
	    }

        public void RemoveGroup(Group g)
	    {
	        _service.RemoveGroup(g.ID);
	        _state.Groups.Remove(g);
	    }


        private void RemoveInstanceFromCache(Instance ins)
	    {
            // remove from instances
	        _state.Instances.Remove(ins.ID);
            // cleanup groups
            foreach (var g in _state.Groups)
	            g.Instances.Remove(ins);
            // claenup metrics
	        _state.Metrics = _state.Metrics.Where(m => m.Instance != ins).ToList();
        }

	} //end of class
}
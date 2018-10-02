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
        private List<Group> _groups = new List<Group>();
		private List<Source> _sources = new List<Source>();
	    private List<Metric> _metrics = new List<Metric>();
		private Dictionary<int, Instance> _instances= new Dictionary<int, Instance>();

        private List<SourceItem> _sourceItems = new List<SourceItem>();


        private readonly Source _unknownSource;
		private readonly Instance _unknownInstance;

		public SourcesCache(Uri url, Func<Uri, IMonikService> serviceFactory)
		{
			_service = serviceFactory(url);

			_unknownSource = new Source {ID = -1, Name = "_UNKNOWN_"};
			_unknownInstance = new Instance {ID = -1, Name = "_UNKNOWN_", Source = _unknownSource};
		}

	    public bool Loaded { get; set; }
	    public async Task Load()
	    {
	        await Task.Run(() => LoadSync());
        }

        public IMonikService Service => _service;

        private void LoadSync()
		{
            var sources = _service.GetSources();
		    var instances = _service.GetInstances();
		    var metrics = _service.GetMetrics();
            var groups = _service.GetGroups();


			_sources = sources.Select(x => new Source
			{
				ID = x.ID,
				Name = x.Name
			}).ToList();

			_instances = new Dictionary<int, Instance>();
			foreach (var it in instances)
			{
				var src = _sources.FirstOrDefault(x => x.ID == it.SourceID) ?? _unknownSource;

				var instance = new Instance
				{
					ID = it.ID,
					Name = it.Name,
					Source = src
				};

				_instances.Add(instance.ID, instance);
			}

		    _metrics = metrics.Select(m =>
		        new Metric
		        {
		            ID = m.ID,
		            Name = m.Name,
		            Instance = _instances.ContainsKey(m.InstanceID) ? _instances[m.InstanceID] : _unknownInstance,
		            Aggregation = m.Aggregation
		        }
		    ).ToList();

            var instancesInGroup = new HashSet<int>();

			_groups = new List<Group>();
			foreach (var it in groups)
			{
				var gr = new Group
				{
					ID = it.ID,
					IsDefault = it.IsDefault,
					Name = it.Name,
					Instances = it.Instances
						.Where(v => _instances.ContainsKey(v))
						.Select(v => _instances[v])
					    .ToList()
				};

                instancesInGroup.UnionWith(it.Instances);

				_groups.Add(gr);
			}

		    _sourceItems = _groups
		        .SelectMany(gr => gr.Instances,
		            (gr, inst) => new SourceItem
		            {
		                GroupID = gr.ID,
		                GroupName = gr.Name,
		                SourceName = inst.Source.Name,
		                InstanceName = inst.Name,
		                InstanceID = inst.ID
		            })
		        .Concat(_instances.Values
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
        }

	    public Group[] Groups => _groups.ToArray();

		public Source[] Sources => _sources.ToArray();

		public Instance[] Instances => _instances.Values.ToArray();

	    public Metric[] Metrics => _metrics.ToArray();

        public SourceItem[] SourceItems => _sourceItems.ToArray();

	    public void RemoveSource(Source v)
	    {
	        _service.RemoveSource(v.ID);
	        
	        _sources.Remove(v);
            foreach(var ins in _instances.Values.ToArray())
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
	        _metrics.Remove(v);
	    }

	    public Instance GetInstance(int aInstanceId)
		{
			return _instances.ContainsKey(aInstanceId)
				? _instances[aInstanceId]
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

            _groups.Add(gr);
	        return gr;
	    }

        public void RemoveGroup(Group g)
	    {
	        _service.RemoveGroup(g.ID);
	        _groups.Remove(g);
	    }


        private void RemoveInstanceFromCache(Instance ins)
	    {
            // remove from instances
	        _instances.Remove(ins.ID);
            // cleanup groups
            foreach (var g in _groups)
	            g.Instances.Remove(ins);
            // claenup metrics
	        _metrics = _metrics.Where(m => m.Instance != ins).ToList();
        }

	} //end of class
}
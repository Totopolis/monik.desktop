using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;
using System.Collections.Generic;
using System.Linq;

namespace MonikDesktop.Common
{
    public class SourcesCache : ISourcesCache
	{
		private readonly IMonikService _service;
	    private List<Group> _groups = new List<Group>();
		private List<Source> _sources = new List<Source>();
	    private List<Metric> _metrics = new List<Metric>();
		private Dictionary<int, Instance> _instances= new Dictionary<int, Instance>();

		private readonly Source _unknownSource;
		private readonly Instance _unknownInstance;

		public SourcesCache(IMonikService aService)
		{
			_service = aService;

			_unknownSource = new Source {ID = -1, Name = "_UNKNOWN_"};
			_unknownInstance = new Instance {ID = -1, Name = "_UNKNOWN_", Source = _unknownSource};
		}

	    public void Reload()
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

			_groups = new List<Group>();
			foreach (var it in groups)
			{
				var gr = new Group
				{
					ID = it.ID,
					IsDefault = it.IsDefault,
					Name = it.Name,
					Instances = it.Instances
						.Where(v => _instances.Keys.Count(x => x == v) > 0)
						.Select(v => _instances.Values.First(x => x.ID == v)).ToList()
				};

				_groups.Add(gr);
			}
		}

		public Group[] Groups => _groups.ToArray();

		public Source[] Sources => _sources.ToArray();

		public Instance[] Instances => _instances.Values.ToArray();

	    public Metric[] Metrics => _metrics.ToArray();

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
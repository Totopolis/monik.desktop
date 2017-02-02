using System.Collections.Generic;
using System.Linq;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;

namespace MonikDesktop.Common
{
	public class SourcesCache : ISourcesCache
	{
		private readonly List<Group> _groups;

		private readonly Dictionary<int, Instance> _instances;

		private readonly List<Source> _sources;
		private readonly Instance _unknownInstance;

		public SourcesCache(IMonikService aService)
		{
			var unknownSource = new Source {ID = -1, Name = "_UNKNOWN_"};
			_unknownInstance = new Instance {ID = -1, Name = "_UNKNOWN_", Source = unknownSource};

			var sources = aService.GetSources();
			var instances = aService.GetInstances();
			var groups = aService.GetGroups();

			_sources = sources.Select(x => new Source
			{
				ID = x.ID,
				Name = x.Name
			}).ToList();

			_instances = new Dictionary<int, Instance>();
			foreach (var it in instances)
			{
				var src = _sources.FirstOrDefault(x => x.ID == it.SourceID) ?? unknownSource;

				var instance = new Instance
				{
					ID = it.ID,
					Name = it.Name,
					Source = src
				};

				_instances.Add(instance.ID, instance);
			}

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

		public Instance GetInstance(int aInstanceId)
		{
			return _instances.ContainsKey(aInstanceId)
				? _instances[aInstanceId]
				: _unknownInstance;

			// TODO: if unknown instance then update from api?
		}
	} //end of class
}
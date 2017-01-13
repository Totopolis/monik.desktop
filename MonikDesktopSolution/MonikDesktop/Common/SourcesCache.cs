using System.Collections.Generic;
using System.Linq;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;

namespace MonikDesktop.Common
{
	public class SourcesCache : ISourcesCache
	{
		private readonly List<Group> FGroups;

		private readonly Dictionary<int, Instance> FInstances;
		private readonly IMonikService FService;

		private readonly List<Source> FSources;
		private readonly Instance FUnknownInstance;

		private readonly Source FUnknownSource;

		public SourcesCache(IMonikService aService)
		{
			FService = aService;

			FUnknownSource = new Source {ID = -1, Name = "_UNKNOWN_"};
			FUnknownInstance = new Instance {ID = -1, Name = "_UNKNOWN_", Source = FUnknownSource};

			var _sources = FService.GetSources();
			var _instances = FService.GetInstances();
			var _groups = FService.GetGroups();

			FSources = _sources.Select(x => new Source
			{
				ID = x.ID,
				Name = x.Name
			}).ToList();

			FInstances = new Dictionary<int, Instance>();
			foreach (var it in _instances)
			{
				var _src = FSources.FirstOrDefault(x => x.ID == it.SourceID);
				if (_src == null)
					_src = FUnknownSource;

				var _instance = new Instance
				{
					ID = it.ID,
					Name = it.Name,
					Source = _src
				};

				FInstances.Add(_instance.ID, _instance);
			}

			FGroups = new List<Group>();
			foreach (var it in _groups)
			{
				var _gr = new Group
				{
					ID = it.ID,
					IsDefault = it.IsDefault,
					Name = it.Name
				};

				_gr.Instances = it.Instances
					.Where(v => FInstances.Keys.Count(x => x == v) > 0)
					.Select(v => FInstances.Values.First(x => x.ID == v)).ToList();

				FGroups.Add(_gr);
			}
		}

		public Group[] Groups
		{
			get { return FGroups.ToArray(); }
		}

		public Source[] Sources
		{
			get { return FSources.ToArray(); }
		}

		public Instance[] Instances
		{
			get { return FInstances.Values.ToArray(); }
		}

		public Instance GetInstance(int aInstanceID)
		{
			return FInstances.ContainsKey(aInstanceID)
				? FInstances[aInstanceID]
				: FUnknownInstance;

			// TODO: if unknown instance then update from api?
		}
	} //end of class
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonikDesktop
{
  public class SourcesCache : ISourcesCache
  {
    private IMonikService FService;

    private Source FUnknownSource;
    private Instance FUnknownInstance;

    private List<Group> FGroups;
    public Group[] Groups { get { return FGroups.ToArray(); } }

    private List<Source> FSources;
    public Source[] Sources { get { return FSources.ToArray(); } }

    private Dictionary<int, Instance> FInstances;
    public Instance[] Instances { get { return FInstances.Values.ToArray(); } }

    public SourcesCache(IMonikService aService)
    {
      FService = aService;

      FUnknownSource = new Source() { ID = -1, Name = "_UNKNOWN_" };
      FUnknownInstance = new Instance() { ID = -1, Name = "_UNKNOWN_", Source = FUnknownSource };

      var _sources = FService.GetSources();
      var _instances = FService.GetInstances();
      var _groups = FService.GetGroups();

      FSources = _sources.Select(x => new Source()
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

        Instance _instance = new Instance()
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
        Group _gr = new Group()
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

    public Instance GetInstance(int aInstanceID)
    {
      return FInstances.ContainsKey(aInstanceID) ? 
        FInstances[aInstanceID] : 
        FUnknownInstance;

      // TODO: if unknown instance then update from api?
    }
  }//end of class
}

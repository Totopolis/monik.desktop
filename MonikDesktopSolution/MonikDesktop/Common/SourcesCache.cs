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
    private List<Source> FSources;
    private Dictionary<int, Instance> FInstances;

    public SourcesCache(IMonikService aService)
    {
      FService = aService;

      FUnknownSource = new Source() { ID = -1, Name = "_UNKNOWN_" };
      FUnknownInstance = new Instance() { ID = -1, Name = "_UNKNOWN_", Source = FUnknownSource };

      var _sources = FService.GetSources();
      var _instances = FService.GetInstances();

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
    }

    public Instance GetInstance(int aInstanceID)
    {
      return FInstances.ContainsKey(aInstanceID) ? 
        FInstances[aInstanceID] : 
        FUnknownInstance;

      // TODO: if unknown instance then update from api
    }
  }//end of class
}

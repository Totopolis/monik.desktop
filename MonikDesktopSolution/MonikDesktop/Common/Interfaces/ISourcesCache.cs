using MonikDesktop.Common.ModelsApp;
using System;
using System.Threading.Tasks;

namespace MonikDesktop.Common.Interfaces
{
    public interface ISourcesCache
	{
	    event Action Loaded;
        bool IsLoaded { get; set; }
	    Task Load();

	    IMonikService Service { get; }

		Group[] Groups { get; }
		Source[] Sources { get; }
		Instance[] Instances { get; }
        Metric[] Metrics { get; }

        SourceItem[] SourceItems { get; }

	    void RemoveSource(Source v);
	    void RemoveInstance(Instance v);
	    void RemoveMetric(Metric v);

	    Group GetGroup(short groupId);
		Instance GetInstance(int aInstanceId);

	    void AddInstanceToGroup(Instance i, Group g);
	    void RemoveInstanceFromGroup(Instance i, Group g);
	    Group CreateGroup(string name, bool isDefault, string description);
	    void RemoveGroup(Group g);
	}
}
using DynamicData;
using MonikDesktop.Common.ModelsApp;
using System.Threading.Tasks;

namespace MonikDesktop.Common.Interfaces
{
    public interface ISourcesCache
	{
	    bool IsLoaded { get; set; }
	    Task Load();

	    IMonikService Service { get; }

	    SourceCache<Group, short> Groups { get; }
	    SourceCache<Source, short> Sources { get; }
	    SourceCache<Metric, int> Metrics { get; }
	    SourceCache<Instance, int> Instances { get; }
        SourceCache<Instance, int> InstancesWithoutGroup { get; }
	    SourceCache<SourceItem, int> SourceItems { get; }

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
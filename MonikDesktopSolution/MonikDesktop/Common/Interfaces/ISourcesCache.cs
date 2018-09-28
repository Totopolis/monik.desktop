using MonikDesktop.Common.ModelsApp;

namespace MonikDesktop.Common.Interfaces
{
	public interface ISourcesCache
	{
		Group[] Groups { get; }
		Source[] Sources { get; }
		Instance[] Instances { get; }
        Metric[] Metrics { get; }

	    void RemoveSource(Source v);
	    void RemoveInstance(Instance v);
	    void RemoveMetric(Metric v);

		Instance GetInstance(int aInstanceId);

	    void AddInstanceToGroup(Instance i, Group g);
	    void RemoveInstanceFromGroup(Instance i, Group g);
	    Group CreateGroup(string name, bool isDefault, string description);
	    void RemoveGroup(Group g);

        void Reload();
	}
}
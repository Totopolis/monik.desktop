using MonikDesktop.Common.ModelsApp;

namespace MonikDesktop.Common.Interfaces
{
	public interface ISourcesCache
	{
		Group[] Groups { get; }
		Source[] Sources { get; }
		Instance[] Instances { get; }
        Metric[] Metrics { get; }

	    bool RemoveSource(Source v);
	    bool RemoveInstance(Instance v);
	    bool RemoveMetric(Metric v);

		Instance GetInstance(int aInstanceId);

		void Reload();
	}
}
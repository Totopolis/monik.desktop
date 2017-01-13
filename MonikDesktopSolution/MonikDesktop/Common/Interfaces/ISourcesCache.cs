using MonikDesktop.Common.ModelsApp;

namespace MonikDesktop.Common.Interfaces
{
	public interface ISourcesCache
	{
		Group[] Groups { get; }
		Source[] Sources { get; }
		Instance[] Instances { get; }

		Instance GetInstance(int aInstanceID);
	}
}
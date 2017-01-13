using MonikDesktop.Common.ModelsApi;

namespace MonikDesktop.Common.Interfaces
{
	public interface IMonikService
	{
		ESource[] GetSources();
		EInstance[] GetInstances();
		EGroup[] GetGroups();
		ELog_[] GetLogs(ELogRequest aRequest);
	}
}
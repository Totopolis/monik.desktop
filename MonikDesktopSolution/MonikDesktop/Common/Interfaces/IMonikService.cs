namespace MonikDesktop
{
	public interface IMonikService
	{
		ESource[] GetSources();
		EInstance[] GetInstances();
		EGroup[] GetGroups();
		ELog_[] GetLogs(ELogRequest aRequest);
	}
}
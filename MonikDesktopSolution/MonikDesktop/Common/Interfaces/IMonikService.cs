using MonikDesktop.Common.ModelsApi;
using System.Collections.Generic;

namespace MonikDesktop.Common.Interfaces
{
    public interface IMonikService
    {
        ESource[] GetSources();
        EInstance[] GetInstances();
        EGroup[] GetGroups();
        ELog_[] GetLogs(ELogRequest aRequest);
        EKeepAlive_[] GetKeepAlives(EKeepAliveRequest aRequest);

        List<EMetricValue> GetCurrentMetricValues();
        List<EWindowValue> GetWindowMetricValues();

        IEnumerable<EMetricDescription> GetMetricDescriptions();
        EMetricHistory GetMetricHistory(int metricId, int amount);
        EMetricValue GetCurrentMetricValue(int metricId);
    }
}

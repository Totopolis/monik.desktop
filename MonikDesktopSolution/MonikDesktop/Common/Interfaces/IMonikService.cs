using MonikDesktop.Common.ModelsApi;
using System.Collections.Generic;

namespace MonikDesktop.Common.Interfaces
{
    public interface IMonikService
    {
        ESource[] GetSources();
        EInstance[] GetInstances();
        EMetric[] GetMetrics();
        EGroup[] GetGroups();
        ELog_[] GetLogs(ELogRequest aRequest);
        EKeepAlive_[] GetKeepAlives(EKeepAliveRequest aRequest);

        List<EMetricValue> GetCurrentMetricValues();
        List<EWindowValue> GetWindowMetricValues();

        IEnumerable<EMetricDescription> GetMetricDescriptions();
        EMetricHistory GetMetricHistory(int metricId, int amount, int skip);
        EMetricValue GetCurrentMetricValue(int metricId);

        bool RemoveSource(short id);
        bool RemoveInstance(int id);
        bool RemoveMetric(int id);
    }
}

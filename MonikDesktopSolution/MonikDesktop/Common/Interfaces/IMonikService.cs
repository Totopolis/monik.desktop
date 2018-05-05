using System.Collections.Generic;
using MonikDesktop.Common.ModelsApi;
using MonikDesktop.Common.ModelsApp;

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
        IEnumerable<EMetricDescription> GetMetricDescriptions();
        List<EMetricValue> GetHistoryMetricValues(EHistoryMetricsRequest aRequest);
        EMetricValue GetCurrentMetricValue(int descriptionId);
    }
}

using MonikDesktop.Common.ModelsApi;
using System;
using System.Collections.Generic;

namespace MonikDesktop.Common.Interfaces
{
    public interface IMonikService
    {
        Uri ServerUrl { get; }
        string AuthToken { get; set; }

        ESource[] GetSources();
        EInstance[] GetInstances();
        EMetric[] GetMetrics();
        EGroup[] GetGroups();
        ELog_[] GetLogs(ELogRequest aRequest);
        EKeepAlive_[] GetKeepAlives(EKeepAliveRequest aRequest);

        List<EMetricValue> GetCurrentMetricValues();
        List<EWindowValue> GetWindowMetricValues();

        EMetricHistory GetMetricHistory(int metricId, int amount, int skip);
        EMetricValue GetCurrentMetricValue(int metricId);

        void RemoveSource(short id);
        void RemoveInstance(int id);
        void RemoveMetric(int id);

        void AddInstanceToGroup(int iId, short gId);
        void RemoveInstanceFromGroup(int iId, short gId);
        EGroup CreateGroup(EGroupCreateRequest request);
        void RemoveGroup(short gId);
    }
}

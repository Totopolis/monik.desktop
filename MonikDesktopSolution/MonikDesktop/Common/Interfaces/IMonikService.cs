﻿using MonikDesktop.Common.ModelsApi;
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

        void RemoveSource(short id);
        void RemoveInstance(int id);
        void RemoveMetric(int id);

        void AddInstanceToGroup(int iId, short gId);
        void RemoveInstanceFromGroup(int iId, short gId);
    }
}

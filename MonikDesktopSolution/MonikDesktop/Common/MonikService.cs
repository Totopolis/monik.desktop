using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApi;
using Newtonsoft.Json;
using Doaking.Core.Oak;
using MonikDesktop.Common.ModelsApp;

namespace MonikDesktop.Common
{
	public class MonikService : IMonikService
	{
		private readonly IOakApplication _app;

		public MonikService(IOakApplication aApp)
		{
			_app = aApp;
		}

		public EGroup[] GetGroups()
		{
			var json = GetJson("groups");
			var result = JsonConvert.DeserializeObject<EGroup[]>(json);
			return result;
		}

		public EInstance[] GetInstances()
		{
			var json = GetJson("instances");
			var result = JsonConvert.DeserializeObject<EInstance[]>(json);
			return result;
		}

		public ESource[] GetSources()
		{
			var json = GetJson("sources");
			var result = JsonConvert.DeserializeObject<ESource[]>(json);
			return result;
		}

		public ELog_[] GetLogs(ELogRequest aRequest)
		{
			var reqJson = JsonConvert.SerializeObject(aRequest);
			var resJson = PostJson("logs5", reqJson);

			var result = JsonConvert.DeserializeObject<ELog_[]>(resJson);
			return result;
	    }

	    public List<EMetricValue> GetCurrentMetricValues()
	    {
	        var json   = GetJson("currentMetrics");
	        var result = JsonConvert.DeserializeObject<List<EMetricValue>>(json);
	        return result;
        }

	    public IEnumerable<EMetricDescription> GetMetricDescriptions()
	    {
	        var json   = GetJson("metricDescriptions");
	        var result = JsonConvert.DeserializeObject<List<EMetricDescription>>(json);
	        return result;
        }

	    public List<EMetricValue> GetHistoryMetricValues(EHistoryMetricsRequest aRequest)
	    {
	        var reqJson = JsonConvert.SerializeObject(aRequest);
	        var resJson = PostJson("metricHistory", reqJson);

	        var result = JsonConvert.DeserializeObject<List<EMetricValue>>(resJson);
	        return result;
        }

	    public EMetricValue GetCurrentMetricValue(int descriptionId)
	    {
	        var json   = GetJson($"currentMetric/{descriptionId}");
	        var result = JsonConvert.DeserializeObject<EMetricValue>(json);
	        return result;
        }

	    public EKeepAlive_[] GetKeepAlives(EKeepAliveRequest aRequest)
	    {
	        var reqJson = JsonConvert.SerializeObject(aRequest);
	        var resJson = PostJson("keepalive2", reqJson);

	        var result = JsonConvert.DeserializeObject<EKeepAlive_[]>(resJson);
	        return result;
	    }

        private string GetJson(string aMethod)
		{
			var request = (HttpWebRequest) WebRequest.Create(_app.ServerUrl + aMethod);
			request.Method = WebRequestMethods.Http.Get;
			request.Accept = "application/json";

			var response = request.GetResponse();
			using (var sr = new StreamReader(response.GetResponseStream()))
			{
				var json = sr.ReadToEnd();
				return json;
			}
		}

		private string PostJson(string aMethod, string aJson)
		{
			var request = (HttpWebRequest) WebRequest.Create(_app.ServerUrl + aMethod);
			request.Method = WebRequestMethods.Http.Post;
			request.Accept = "application/json";
			request.ContentType = "application/json";

			using (var sw = new StreamWriter(request.GetRequestStream()))
			{
				sw.Write(aJson);
			}

			var response = request.GetResponse();
			using (var sr = new StreamReader(response.GetResponseStream()))
			{
				var json = sr.ReadToEnd();
				return json;
			}
		}
	}
}
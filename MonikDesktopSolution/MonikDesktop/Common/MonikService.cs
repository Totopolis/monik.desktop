using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace MonikDesktop.Common
{
    public class MonikService : IMonikService
	{
		private readonly IAppModel _app;

		public MonikService(IAppModel app)
		{
			_app = app;
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

	    public EMetric[] GetMetrics()
	    {
	        var json = GetJson("metrics");
	        var result = JsonConvert.DeserializeObject<EMetric[]>(json);
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
	        var json   = GetJson("metrics/currents");
	        var result = JsonConvert.DeserializeObject<List<EMetricValue>>(json);
	        return result;
        }

        public List<EWindowValue> GetWindowMetricValues()
        {
            var json = GetJson("metrics/windows");
            var result = JsonConvert.DeserializeObject<List<EWindowValue>>(json);
            return result;
        }

        public IEnumerable<EMetricDescription> GetMetricDescriptions()
	    {
	        var json   = GetJson("metrics");
	        var result = JsonConvert.DeserializeObject<List<EMetricDescription>>(json);
	        return result;
        }

	    public EMetricHistory GetMetricHistory(int metricId, int amount, int skip)
	    {
	        var json = GetJson($"metrics/{metricId}/history?Amount={amount}&Skip={skip}");
	        var result = JsonConvert.DeserializeObject<EMetricHistory>(json);
	        return result;
        }

	    public EMetricValue GetCurrentMetricValue(int metricId)
	    {
	        var json   = GetJson($"metrics/{metricId}/current");
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


	    public void RemoveSource(short id)
	    {
	        Delete($"sources/{id}");
	    }

	    public void RemoveInstance(int id)
	    {
	        Delete($"instances/{id}");
        }

	    public void RemoveMetric(int id)
	    {
	        Delete($"metrics/{id}");
        }

	    public void AddInstanceToGroup(int iId, short gId)
	    {
	        Put($"groups/{gId}/instances/{iId}");
	    }

	    public void RemoveInstanceFromGroup(int iId, short gId)
	    {
	        Delete($"groups/{gId}/instances/{iId}");
        }

	    public EGroup CreateGroup(EGroupCreateRequest request)
	    {
	        var reqJson = JsonConvert.SerializeObject(request);
	        var resJson = PostJson("groups", reqJson, true);

	        var result = JsonConvert.DeserializeObject<EGroup>(resJson);
	        return result;
	    }

	    public void RemoveGroup(short gId)
	    {
            Delete($"groups/{gId}");
	    }

        private void Delete(string aMethod)
	    {
	        var request = CreateRequest(aMethod);
	        request.Method = "DELETE";
            AddAuthorization(request);

	        request.GetResponse();
	    }

        private void Put(string aMethod)
        {
            var request = CreateRequest(aMethod);
            request.Method = "PUT";
            AddAuthorization(request);

            request.ContentLength = 0;

            request.GetResponse();
        }

        private string GetJson(string aMethod)
		{
		    var request = CreateRequest(aMethod);
			request.Method = WebRequestMethods.Http.Get;
			request.Accept = "application/json";

			var response = request.GetResponse();
			using (var sr = new StreamReader(response.GetResponseStream()))
			{
				var json = sr.ReadToEnd();
				return json;
			}
		}

		private string PostJson(string aMethod, string aJson, bool needAuthorization = false)
		{
		    var request = CreateRequest(aMethod);
			request.Method = WebRequestMethods.Http.Post;
			request.Accept = "application/json";
			request.ContentType = "application/json";

            if (needAuthorization)
                AddAuthorization(request);

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

	    private HttpWebRequest CreateRequest(string aMethod)
	    {
	        var uri = new Uri(_app.ServerUrl, aMethod);
	        var request = (HttpWebRequest) WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            return request;
	    }

	    private void AddAuthorization(HttpWebRequest request)
	    {
	        request.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {_app.AuthToken}");
        }
    }
}
using System.IO;
using System.Net;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApi;
using Newtonsoft.Json;

namespace MonikDesktop.Common
{
	public class MonikService : IMonikService
	{
		private readonly OakApplication FApp;

		public MonikService(OakApplication aApp)
		{
			FApp = aApp;
		}

		public EGroup[] GetGroups()
		{
			var _json = GetJson("groups");
			var _res = JsonConvert.DeserializeObject<EGroup[]>(_json);
			return _res;
		}

		public EInstance[] GetInstances()
		{
			var _json = GetJson("instances");
			var _res = JsonConvert.DeserializeObject<EInstance[]>(_json);
			return _res;
		}

		public ESource[] GetSources()
		{
			var _json = GetJson("sources");
			var _res = JsonConvert.DeserializeObject<ESource[]>(_json);
			return _res;
		}

		public ELog_[] GetLogs(ELogRequest aRequest)
		{
			var _reqJson = JsonConvert.SerializeObject(aRequest);
			var _resJson = PostJson("logs5", _reqJson);

			var _res = JsonConvert.DeserializeObject<ELog_[]>(_resJson);
			return _res;
		}

		private string GetJson(string aMethod)
		{
			var _request = (HttpWebRequest) WebRequest.Create(FApp.ServerUrl + aMethod);
			_request.Method = WebRequestMethods.Http.Get;
			_request.Accept = "application/json";

			var _response = _request.GetResponse();
			using (var sr = new StreamReader(_response.GetResponseStream()))
			{
				var _json = sr.ReadToEnd();
				return _json;
			}
		}

		private string PostJson(string aMethod, string aJson)
		{
			var _request = (HttpWebRequest) WebRequest.Create(FApp.ServerUrl + aMethod);
			_request.Method = WebRequestMethods.Http.Post;
			_request.Accept = "application/json";
			_request.ContentType = "application/json";

			using (var sw = new StreamWriter(_request.GetRequestStream()))
			{
				sw.Write(aJson);
			}

			var _response = _request.GetResponse();
			using (var sr = new StreamReader(_response.GetResponseStream()))
			{
				var _json = sr.ReadToEnd();
				return _json;
			}
		}
	}
}
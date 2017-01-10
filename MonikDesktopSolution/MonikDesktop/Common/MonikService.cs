using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MonikDesktop
{
  public class MonikService : IMonikService
  {
    private OakApplication FApp;

    public MonikService(OakApplication aApp)
    {
      FApp = aApp;
    }

    private string GetJson(string aMethod)
    {
      HttpWebRequest _request = (HttpWebRequest)WebRequest.Create(FApp.ServerUrl + aMethod);
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
      HttpWebRequest _request = (HttpWebRequest)WebRequest.Create(FApp.ServerUrl + aMethod);
      _request.Method = WebRequestMethods.Http.Post;
      _request.Accept = "application/json";
      _request.ContentType = "application/json";

      using (var sw = new StreamWriter(_request.GetRequestStream()))
        sw.Write(aJson);

      var _response = _request.GetResponse();
      using (var sr = new StreamReader(_response.GetResponseStream()))
      {
        var _json = sr.ReadToEnd();
        return _json;
      }
    }

    public EGroup[] GetGroups()
    {
      string _json = GetJson("groups");
      EGroup[] _res = JsonConvert.DeserializeObject<EGroup[]>(_json);
      return _res;
    }

    public EInstance[] GetInstances()
    {
      string _json = GetJson("instances");
      EInstance[] _res = JsonConvert.DeserializeObject<EInstance[]>(_json);
      return _res;
    }

    public ESource[] GetSources()
    {
      string _json = GetJson("sources");
      ESource[] _res = JsonConvert.DeserializeObject<ESource[]>(_json);
      return _res;
    }

    public ELog_[] GetLogs(ELogRequest aRequest)
    {
      string _reqJson = JsonConvert.SerializeObject(aRequest);
      string _resJson = PostJson("logs5", _reqJson);

      ELog_[] _res = JsonConvert.DeserializeObject<ELog_[]>(_resJson);
      return _res;
    }
  }
}

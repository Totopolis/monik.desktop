using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MonikDesktop.Common.Config
{
    public class AppConfigStorage
    {
        private readonly FileInfo _fileInfo;
        
        public AppConfigStorage()
        {
            _fileInfo = new FileInfo(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"MonikDesktop\app.config"));
            _fileInfo.Directory?.Create();
        }

        public void Save(AppConfig value)
        {
            var data = JsonConvert.SerializeObject(value);
            File.WriteAllText(_fileInfo.ToString(), data, Encoding.UTF8);
        }

        public AppConfig Load()
        {
            if (!_fileInfo.Exists)
            {
                return new AppConfig();
            }

            var data = File.ReadAllText(_fileInfo.ToString(), Encoding.UTF8);
            return JsonConvert.DeserializeObject<AppConfig>(data);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using MonikDesktop.Common.ModelsApi;
using MonikDesktop.Common.ModelsApp;
using MonikDesktop.ViewModels.ShowModels;

namespace SampleDatas
{
    public class KeepAliveSamples
    {
        public List<KeepALiveItem> KeepALiveList
        {
            get
            {
                var dateTime = DateTime.Now;

                return new List<KeepALiveItem>()
                {
                    new KeepALiveItem()
                    {
                        Created    = dateTime,
                        CreatedStr = dateTime.ToString("hh:mm:ss"),
                        Instance = new Instance()
                        {
                            ID   = -1,
                            Name = "TestInstOk",
                            Source = new Source()
                            {
                                ID   = -1,
                                Name = "TestSourceOk"
                            },
                        },
                        Status     = "OK",
                        StatusIsOk = true
                    },
                    new KeepALiveItem()
                    {
                        Created    = dateTime,
                        CreatedStr = dateTime.ToString("hh:mm:ss"),
                        Instance = new Instance()
                        {
                            ID   = -1,
                            Name = "TestInstErr",
                            Source = new Source()
                            {
                                ID   = -1,
                                Name = "TestSourceErr"
                            },
                        },
                        Status     = "ERROR",
                        StatusIsOk = false
                    },
                };
            }
        }
        
        public KeepAliveModel Model => throw new NotImplementedException();
    }
}
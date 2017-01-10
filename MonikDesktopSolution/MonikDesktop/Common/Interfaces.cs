using MonikDesktop.Oak;
using MonikDesktop.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonikDesktop
{
  public interface IMonikService
  {
    ESource[] GetSources();
    EInstance[] GetInstances();
    EGroup[] GetGroups();
    ELog_[] GetLogs(ELogRequest aRequest);
  }

  public interface ISourcesCache
  {
    Group[] Groups { get; }
    Source[] Sources { get; }
    Instance[] Instances { get; }

    Instance GetInstance(int aInstanceID);
  }
  
  public interface IStartupWindow : IDockingWindow
  {
  }

  public interface IShowWindow : IDockingWindow
  {
    ReactiveCommand StartCommand { get; set; }
    ReactiveCommand StopCommand { get; set; }

    ShowModel Model { get; }
  }

  public enum SeverityCutoffType : byte
  {
    None = 255,
    Info = 30,
    Warning = 20,
    Error = 10,
    Fatal = 0
  }

  public enum LevelType : byte
  {
    None = 255,
    System = 0,
    Application = 10,
    Logic = 20,
    Security = 30
  }

  public enum TopType : int
  {
    None = 0,
    Top50 = 50,
    Top100 = 100,
    Top500 = 500,
    Top1000 = 1000
  }

  public interface ILogsWindow : IShowWindow
  {
  }

  public interface IKeepAliveWindow : IShowWindow
  {
  }

  public interface ISourcesWindow : IDockingWindow
  {
  }

  public interface IPropertiesWindow : IDockingWindow
  {
  }
}

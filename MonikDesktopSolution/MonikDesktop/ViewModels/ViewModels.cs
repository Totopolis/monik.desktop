using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop.ViewModels
{
  public class ShowModel : ReactiveObject
  {
    public ReactiveList<short> Groups { get; } = new ReactiveList<short>() { ChangeTrackingEnabled = true };
    public ReactiveList<int> Instances { get; } = new ReactiveList<int>() { ChangeTrackingEnabled = true };

    [Reactive]
    public string Caption { get; set; } = "";

    [Reactive]
    public bool Online { get; set; } = false;

    [Reactive]
    public int RefreshSec { get; set; } = 3;

    [Reactive]
    public bool Colorized { get; set; } = true;

    [Reactive]
    public string DateTimeFormat { get; set; } = "HH:mm:ss";

    protected ShowModel()
    {
      Groups.CountChanged.Subscribe(_ => Online = false);
      Instances.CountChanged.Subscribe(_ => Online = false);
      this.ObservableForProperty(x => x.RefreshSec).Subscribe(_ => Online = false);
      this.ObservableForProperty(x => x.Colorized).Subscribe(_ => Online = false);
      this.ObservableForProperty(x => x.DateTimeFormat).Subscribe(_ => Online = false);
    }
  }

  public class LogsModel : ShowModel
  {
    [Reactive]
    public TopType Top { get; set; } = TopType.Top50;

    [Reactive]
    public SeverityCutoffType SeverityCutoff { get; set; } = SeverityCutoffType.Info;

    [Reactive]
    public LevelType Level { get; set; } = LevelType.None;

    [Reactive]
    public LogItem SelectedItem { get; set; } = null;

    // at offline mode
    //DateTime From { get; set; }
    //DateTime To { get; set; }

    //bool UpDownDirection { get; set; }

    // wrapbody
    // columns

    public LogsModel() : base()
    {
      this.ObservableForProperty(x => x.Level).Subscribe(_ => Online = false);
      this.ObservableForProperty(x => x.Top).Subscribe(_ => Online = false);
      this.ObservableForProperty(x => x.SeverityCutoff).Subscribe(_ => Online = false);
    }
  }

  public class KeepAliveModel : ShowModel
  {
    public KeepAliveModel() : base() { }
  }
}

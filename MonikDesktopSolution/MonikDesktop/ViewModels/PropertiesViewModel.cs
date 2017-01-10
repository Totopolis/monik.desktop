using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using MonikDesktop.Oak;
using System.Reactive.Linq;

namespace MonikDesktop.ViewModels
{
  public class PropertiesViewModel : ReactiveObject, IPropertiesWindow
  {
    public ShowModel Model { get; private set; }
    public IShowWindow ShowWindow { get; private set; }

    public PropertiesViewModel(Shell aShell)
    {
      Title = "Properties";

      aShell.WhenAnyValue(x => x.SelectedWindow)
        .Where(v => v is IShowWindow)
        .Subscribe(v => OnSelectedWindow(v as IShowWindow));
    }

    private void OnSelectedWindow(IShowWindow aWindow)
    {
      Model = aWindow == null ? null : aWindow.Model;
      ShowWindow = aWindow;
    }

    public IList<TopType> TopTypes { get { return Enum.GetValues(typeof(TopType)).Cast<TopType>().ToList<TopType>(); } }
    public IList<SeverityCutoffType> SeverityCutoffTypes { get { return Enum.GetValues(typeof(SeverityCutoffType)).Cast<SeverityCutoffType>().ToList<SeverityCutoffType>(); } }
    public IList<LevelType> LevelTypes { get { return Enum.GetValues(typeof(LevelType)).Cast<LevelType>().ToList<LevelType>(); } }
    public IList<string> DatetimeFormats { get { return new string[] { "HH:mm:ss", "dd.MM.YYYY HH:mm:ss", "dd.MM HH:mm:ss" }; } }

    [Reactive]
    public string Title { get; set; }
    [Reactive]
    public bool CanClose { get; set; } = true;
    [Reactive]
    public ReactiveCommand CloseCommand { get; set; } = null;
  }
}

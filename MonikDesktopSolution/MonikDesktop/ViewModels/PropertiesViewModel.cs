using Gemini.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Framework.Services;
using ReactiveUI;

namespace MonikDesktop.ViewModels
{
  public class PropertiesViewModel : Tool, IPropertiesWindow
  {
    private MApp FApp;
    public ShowModel Model { get; private set; }
    public IShowWindow ShowWindow { get; private set; }

    public PropertiesViewModel(MApp aApp)
    {
      FApp = aApp;
      DisplayName = "Properties";

      aApp.WhenAnyValue(x => x.SelectedWindow).Subscribe(w => OnSelectedWindow(w));
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

    public override PaneLocation PreferredLocation { get { return PaneLocation.Left; } }
  }
}

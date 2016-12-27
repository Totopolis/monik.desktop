using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonikDesktop
{
  public class MApp : ReactiveObject
  {
    [Reactive]
    public string Name { get; set; } = "Monik Desktop";
    [Reactive]
    public IShowWindow SelectedWindow { get; set; } = null;
    [Reactive]
    public string ServerUrl { get; set; } = "";

    public MApp()
    {
    }

  }//end of class
}

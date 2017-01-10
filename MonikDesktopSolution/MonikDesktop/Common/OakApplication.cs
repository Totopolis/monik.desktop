using MonikDesktop.Oak;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonikDesktop
{
  public class OakApplication : ReactiveObject, IOakApplication
  {
    [Reactive]
    public string ServerUrl { get; set; } = "";
    [Reactive]
    public string Title { get; set; } = "Monik Desktop";

    public OakApplication()
    {
    }

  }//end of class
}

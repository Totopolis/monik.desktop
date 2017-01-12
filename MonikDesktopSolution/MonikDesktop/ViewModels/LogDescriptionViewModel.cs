using MonikDesktop.Oak;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonikDesktop.ViewModels
{
  public class LogDescriptionViewModel : ReactiveObject, ILogDescription
  {
    public LogDescriptionViewModel(Shell aShell)
    {
      Title = "Log Description";
    }

    public bool CanClose { get; set; } = false;

    public ReactiveCommand CloseCommand { get; set; } = null;

    [Reactive]
    public LogItem SelectedItem { get; set; } = null;

    public string Title { get; set; }

  }//end of class
}

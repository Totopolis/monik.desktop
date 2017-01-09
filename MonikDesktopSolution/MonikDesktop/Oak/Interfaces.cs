using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonikDesktop.Oak
{
  public interface IDockingWindow
  {
    string Title { get; set; }
    bool CanClose { get; set; }
    ReactiveCommand CloseCommand { get; set; }
  }
}

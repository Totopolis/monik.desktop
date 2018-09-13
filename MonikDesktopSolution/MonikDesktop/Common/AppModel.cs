using MonikDesktop.Common.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop.Common
{
    public class AppModel : ReactiveObject, IAppModel
    {
        [Reactive]
        public string ServerUrl { get; set; }
    }
}

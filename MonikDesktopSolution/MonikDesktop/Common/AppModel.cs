using MonikDesktop.Common.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;

namespace MonikDesktop.Common
{
    public class AppModel : ReactiveObject, IAppModel
    {
        [Reactive] public Uri ServerUrl { get; set; } = null;
        [Reactive] public string AuthToken { get; set; } = null;
    }
}

using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop.ViewModels.ShowModels
{
	public class KeepAliveModel : ShowModel
    {
        public KeepAliveModel()
        {
            this.ObservableForProperty(x => x.KeepAliveWarningPeriod).Subscribe(_ => Online = false);
        }

        [Reactive]
        public int KeepAliveWarningPeriod { get; set; } = 70;

    }
}
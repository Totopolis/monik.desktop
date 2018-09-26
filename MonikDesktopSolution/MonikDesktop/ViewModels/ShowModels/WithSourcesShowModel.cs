using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;

namespace MonikDesktop.ViewModels.ShowModels
{
    public class WithSourcesShowModel : ShowModel
    {
        [Reactive]
        public ReactiveList<short> Groups { get; set; }

        [Reactive]
        public ReactiveList<int> Instances { get; set; }

        public WithSourcesShowModel()
        {
            Groups = new ReactiveList<short> { ChangeTrackingEnabled = true };
            Instances = new ReactiveList<int> { ChangeTrackingEnabled = true };

            Groups.CountChanged.Subscribe(_ => Online = false);
            Instances.CountChanged.Subscribe(_ => Online = false);
        }
    }
}

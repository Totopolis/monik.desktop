using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop.ViewModels.ShowModels
{
	public class ShowModel : ReactiveObject
	{
		protected ShowModel()
		{
			Groups = new ReactiveList<short> { ChangeTrackingEnabled = true };
			Instances = new ReactiveList<int> { ChangeTrackingEnabled = true };

			Groups.CountChanged.Subscribe(_ => Online = false);
			Instances.CountChanged.Subscribe(_ => Online = false);
			this.ObservableForProperty(x => x.RefreshSec).Subscribe(_ => Online = false);
			this.ObservableForProperty(x => x.Colorized).Subscribe(_ => Online = false);
			this.ObservableForProperty(x => x.DateTimeFormat).Subscribe(_ => Online = false);
		}

		[Reactive]
		public ReactiveList<short> Groups { get; set; }

		[Reactive]
		public ReactiveList<int> Instances { get; set; }

		[Reactive]
		public string Caption { get; set; } = "";

		[Reactive]
		public bool Online { get; set; }

		[Reactive]
		public int RefreshSec { get; set; } = 3;

		[Reactive]
		public bool Colorized { get; set; } = true;

        [Reactive]
		public string DateTimeFormat { get; set; } = "HH:mm:ss";

	    [Reactive]
	    public bool GroupDuplicatingItems { get; set; } = true;

        [Reactive]
		public string DuplicatedDateTimeFormat { get; set; } = "HH:mm:**";
	}
}
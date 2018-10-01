using MonikDesktop.Common.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;

namespace MonikDesktop.ViewModels.ShowModels
{
    public class ShowModel : ReactiveObject
	{
		protected ShowModel()
		{
			this.ObservableForProperty(x => x.RefreshSec).Subscribe(_ => Online = false);
			this.ObservableForProperty(x => x.DateTimeFormat).Subscribe(_ => Online = false);
		}

        public ISourcesCache Cache { get; set; }

		[Reactive]
		public string Caption { get; set; } = "";

		[Reactive]
		public bool Online { get; set; }

		[Reactive]
		public int RefreshSec { get; set; } = 3;

        [Reactive]
		public string DateTimeFormat { get; set; } = "HH:mm:ss";
	}
}
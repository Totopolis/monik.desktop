using MonikDesktop.Common.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Disposables;

namespace MonikDesktop.ViewModels.ShowModels
{
    public class ShowModel : ReactiveObject, IDisposable
	{
		protected ShowModel()
		{
			this.ObservableForProperty(x => x.RefreshSec).Subscribe(_ => Online = false);
			this.ObservableForProperty(x => x.DateTimeFormat).Subscribe(_ => Online = false);
		}

	    public virtual ISourcesCache Cache { get; set; }

	    [Reactive]
		public string Caption { get; set; } = "";

		[Reactive]
		public bool Online { get; set; }

		[Reactive]
		public int RefreshSec { get; set; } = 3;

        [Reactive]
		public string DateTimeFormat { get; set; } = "HH:mm:ss";

        protected CompositeDisposable Disposables = new CompositeDisposable();
        
	    public void Dispose()
	    {
	        Disposables?.Dispose();
	    }
	}
}
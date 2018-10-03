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

	    protected virtual void OnCacheLoaded()
	    {
	        Online = false;
	    }
	    private ISourcesCache _cache;
        public ISourcesCache Cache
        {
            get => _cache;
            set
            {
                if (_cache != null)
                    _cache.Loaded -= OnCacheLoaded;

                _cache = value;

                if (_cache != null)
                    _cache.Loaded += OnCacheLoaded;
            }
        }

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
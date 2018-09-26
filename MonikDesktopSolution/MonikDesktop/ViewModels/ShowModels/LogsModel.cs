using System;
using System.Windows;
using MonikDesktop.Common.Enums;
using MonikDesktop.Common.ModelsApp;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop.ViewModels.ShowModels
{
	public class LogsModel : WithSourcesShowModel
	{
		// at offline mode
		//DateTime From { get; set; }
		//DateTime To { get; set; }

		//bool UpDownDirection { get; set; }

		// wrapbody
		// columns

		public LogsModel()
		{
            this.ObservableForProperty(x => x.Colorized).Subscribe(_ => Online = false);

            this.ObservableForProperty(x => x.Level)                        .Subscribe(_ => Online = false);
			this.ObservableForProperty(x => x.Top)                          .Subscribe(_ => Online = false);
			this.ObservableForProperty(x => x.SeverityCutoff)               .Subscribe(_ => Online = false);
			this.ObservableForProperty(x => x.GroupDuplicatingItems)        .Subscribe(_ =>
			{
			    Online = false;
			    DuplicatedSettingsVisibility = GroupDuplicatingItems ? Visibility.Visible : Visibility.Collapsed;
			});
			this.ObservableForProperty(x => x.DuplicatedDateTimeFormat)     .Subscribe(_ => Online = false);
			this.ObservableForProperty(x => x.DuplicatedSettingsVisibility) .Subscribe(_ => Online = false);
		}

	    [Reactive]
	    public bool Colorized { get; set; } = true;

        [Reactive]
		public TopType Top { get; set; } = TopType.Top50;

		[Reactive]
		public SeverityCutoffType SeverityCutoff { get; set; } = SeverityCutoffType.Info;
        
        [Reactive]
		public LevelType Level { get; set; } = LevelType.None;

		[Reactive]
		public LogItem SelectedItem { get; set; } = null;

	    [Reactive]
	    public bool GroupDuplicatingItems { get; set; } = true;

	    [Reactive]
	    public string DuplicatedDateTimeFormat { get; set; } = "HH:mm:**";

        [Reactive]
        public Visibility DuplicatedSettingsVisibility { get; set; } = Visibility.Visible;
    }
}
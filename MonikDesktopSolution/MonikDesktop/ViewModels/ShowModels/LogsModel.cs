using System;
using System.Windows;
using MonikDesktop.Common.Enums;
using MonikDesktop.Common.ModelsApp;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop.ViewModels.ShowModels
{
	public class LogsModel : ShowModel
	{
		// at offline mode
		//DateTime From { get; set; }
		//DateTime To { get; set; }

		//bool UpDownDirection { get; set; }

		// wrapbody
		// columns

		public LogsModel()
		{
		    Groups = new ReactiveList<short> { ChangeTrackingEnabled = true };
		    Instances = new ReactiveList<int> { ChangeTrackingEnabled = true };

		    Groups.CountChanged.Subscribe(_ => Online = false);
		    Instances.CountChanged.Subscribe(_ => Online = false);

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
	    public ReactiveList<short> Groups { get; set; }

	    [Reactive]
	    public ReactiveList<int> Instances { get; set; }

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
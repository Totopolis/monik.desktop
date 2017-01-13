using System;
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
			this.ObservableForProperty(x => x.Level).Subscribe(_ => Online = false);
			this.ObservableForProperty(x => x.Top).Subscribe(_ => Online = false);
			this.ObservableForProperty(x => x.SeverityCutoff).Subscribe(_ => Online = false);
		}

		[Reactive]
		public TopType Top { get; set; } = TopType.Top50;

		[Reactive]
		public SeverityCutoffType SeverityCutoff { get; set; } = SeverityCutoffType.Info;

		[Reactive]
		public LevelType Level { get; set; } = LevelType.None;

		[Reactive]
		public LogItem SelectedItem { get; set; } = null;
	}
}
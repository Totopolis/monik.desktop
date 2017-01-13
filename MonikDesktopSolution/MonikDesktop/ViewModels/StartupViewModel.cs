using System;
using Autofac;
using MonikDesktop.Oak;
using MonikDesktop.Properties;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop.ViewModels
{
	public class StartupViewModel : ReactiveObject, IStartupWindow
	{
		private readonly Shell FShell;

		public StartupViewModel(OakApplication aApp, Shell aShell)
		{
			FShell = aShell;
			App = aApp;

			Title = "App settings";

			App.ObservableForProperty(x => x.ServerUrl)
				.Subscribe(v =>
				{
					Settings.Default.ServerUrl = v.Value;
					Settings.Default.Save();
				});

			var _canNew = App.WhenAny(x => x.ServerUrl, x => !string.IsNullOrWhiteSpace(x.Value));
			NewLogCommand = ReactiveCommand.Create(NewLog, _canNew);
			NewKeepAliveCommand = ReactiveCommand.Create(NewLog, _canNew);
		}

		public OakApplication App { get; }

		public ReactiveCommand NewLogCommand { get; set; }
		public ReactiveCommand NewKeepAliveCommand { get; set; }

		[Reactive]
		public bool CanClose { get; set; } = false;

		[Reactive]
		public ReactiveCommand CloseCommand { get; set; } = null;

		[Reactive]
		public string Title { get; set; }

		private void NewLog()
		{
			// TODO: check server url

			var _log = Bootstrap.Container.Resolve<ILogsWindow>();
			var _props = Bootstrap.Container.Resolve<IPropertiesWindow>();
			var _sources = Bootstrap.Container.Resolve<ISourcesWindow>();
			var _desc = Bootstrap.Container.Resolve<ILogDescription>();

			FShell.ShowDocument(_log);
			FShell.ShowTool(_props);
			FShell.ShowTool(_sources);
			FShell.ShowTool(_desc);

			FShell.SelectedWindow = _log;
		}
	}
}
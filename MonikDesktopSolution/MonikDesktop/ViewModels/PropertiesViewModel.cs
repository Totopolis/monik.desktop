using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Doaking.Core.Oak;
using MonikDesktop.Common.Enums;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.ViewModels.ShowModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop.ViewModels
{
	public class PropertiesViewModel : ReactiveObject, IPropertiesWindow
	{
		public PropertiesViewModel(Shell aShell)
		{
			Title = "Properties";

			aShell.WhenAnyValue(x => x.SelectedWindow)
				.Where(v => v is IShowWindow)
				.Subscribe(v => OnSelectedWindow(v as IShowWindow));
		}

		[Reactive]
		public ShowModel Model { get; private set; }

		[Reactive]
		public IShowWindow ShowWindow { get; private set; }

		public IList<TopType> TopTypes
		{
			get { return Enum.GetValues(typeof(TopType)).Cast<TopType>().ToList(); }
		}

		public IList<SeverityCutoffType> SeverityCutoffTypes
		{
			get { return Enum.GetValues(typeof(SeverityCutoffType)).Cast<SeverityCutoffType>().ToList(); }
		}

		public IList<LevelType> LevelTypes
		{
			get { return Enum.GetValues(typeof(LevelType)).Cast<LevelType>().ToList(); }
		}

		public IList<string> DatetimeFormats
		{
			get { return new[] {"HH:mm:ss", "dd.MM.yyyy HH:mm:ss", "dd.MM HH:mm:ss"}; }
		}

		[Reactive]
		public string Title { get; set; }

		[Reactive]
		public bool CanClose { get; set; } = false;

		[Reactive]
		public ReactiveCommand CloseCommand { get; set; } = null;

		private void OnSelectedWindow(IShowWindow aWindow)
		{
			Model = aWindow == null ? null : aWindow.Model;
			ShowWindow = aWindow;
		}
	}
}
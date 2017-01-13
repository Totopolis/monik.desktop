using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;

namespace Doaking.Core.Oak
{
	// http://stackoverflow.com/questions/12493117/wpf-avalondock-v2-0-layoutdocument
	public class Shell : ReactiveObject
	{
		private DockingManager FDocker;
		private LayoutDocumentPane FDocumentPane;
		private LayoutRoot FLayoutRoot;
		private LayoutAnchorablePane FLeftPane;

		private readonly Dictionary<Type, Type> FModelViews;
		private readonly ReactiveList<IDockingWindow> FWindows;

		public Shell()
		{
			FWindows = new ReactiveList<IDockingWindow>();
			FModelViews = new Dictionary<Type, Type>();
		}

		[Reactive]
		public IDockingWindow SelectedWindow { get; set; } = null;

		public void RegisterModelView<MODEL, VIEW>()
			where MODEL : IDockingWindow
			where VIEW : UserControl
		{
			FModelViews.Add(typeof(MODEL), typeof(VIEW));
		}

		public void AttachDocker(DockingManager aDocker)
		{
			FDocker = aDocker;

			FLayoutRoot = new LayoutRoot();
			FDocker.Layout = FLayoutRoot;

			FDocumentPane = FLayoutRoot.RootPanel.Children[0] as LayoutDocumentPane;

			FLeftPane = new LayoutAnchorablePane();
			FLayoutRoot.RootPanel.Children.Insert(0, FLeftPane);
			FLeftPane.DockWidth = new GridLength(410);
		}

		public void ShowTool(IDockingWindow aWindow)
		{
			if (FWindows.Contains(aWindow))
				return;

			var _view = CreateView(aWindow);
			FWindows.Add(aWindow);

			var _layoutAnchorable = new LayoutAnchorable();

			aWindow.WhenAnyValue(x => x.Title)
				.Subscribe(v => _layoutAnchorable.Title = v);

			aWindow.WhenAnyValue(x => x.CanClose)
				.Subscribe(v =>
				{
					_layoutAnchorable.CanHide = v;
					_layoutAnchorable.CanClose = v;
				});

			_layoutAnchorable.Content = _view;

			FLeftPane.Children.Add(_layoutAnchorable);
		}

		public void ShowDocument(IDockingWindow aWondow)
		{
			var _view = CreateView(aWondow);
			FWindows.Add(aWondow);

			var _layoutDocument = new LayoutDocument();

			aWondow.WhenAnyValue(x => x.Title)
				.Subscribe(v => _layoutDocument.Title = v);

			aWondow.WhenAnyValue(x => x.CanClose)
				.Subscribe(v => _layoutDocument.CanClose = v);

			_layoutDocument.Content = _view;

			FDocumentPane.Children.Add(_layoutDocument);

			_layoutDocument.IsActive = true;
		}

		private UserControl CreateView(IDockingWindow aModel)
		{
			var _types = aModel.GetType().GetInterfaces();

			Type _type = null;

			foreach (var it in FModelViews.Keys)
				if (_types.Contains(it))
					_type = FModelViews[it];

			if (_type == null)
				throw new Exception($"Model {aModel} not found");

			var _view = Activator.CreateInstance(_type) as UserControl;
			if (_view == null)
				throw new Exception($"Cant create view for type {_type}");

			_view.DataContext = aModel;

			return _view;
		}
	}
}
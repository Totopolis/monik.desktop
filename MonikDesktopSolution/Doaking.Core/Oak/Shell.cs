using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Autofac;

namespace Doaking.Core.Oak
{
	// http://stackoverflow.com/questions/12493117/wpf-avalondock-v2-0-layoutdocument
	public class Shell : ReactiveObject
	{
		private DockingManager _docker;
		private LayoutDocumentPane _documentPane;
		private LayoutRoot _layoutRoot;
		private LayoutAnchorablePane _leftPane;

		private readonly Dictionary<Type, Type> _modelViews;
		private readonly ReactiveList<IDockingWindow> _windows;

		public Shell()
		{
			_windows = new ReactiveList<IDockingWindow>();
			_modelViews = new Dictionary<Type, Type>();
			Container = null;
		}

		public IContainer Container { get; set; }

		[Reactive]
		public IDockingWindow SelectedWindow { get; set; } = null;

		public void RegisterModelView<TModel, TView>()
			where TModel : IDockingWindow
			where TView : UserControl
		{
			_modelViews.Add(typeof(TModel), typeof(TView));
		}

		public T Resolve<T>()
		{
			if (Container == null)
				throw new Exception("Container not initialized");

			return Container.Resolve<T>();
		}

		public void AttachDocker(DockingManager aDocker)
		{
			_docker = aDocker;

			_layoutRoot = new LayoutRoot();
			_docker.Layout = _layoutRoot;

			_documentPane = _layoutRoot.RootPanel.Children[0] as LayoutDocumentPane;

			_leftPane = new LayoutAnchorablePane();
			_layoutRoot.RootPanel.Children.Insert(0, _leftPane);
			_leftPane.DockWidth = new GridLength(410);
		}

		public void ShowTool(IDockingWindow aWindow)
		{
			if (_windows.Contains(aWindow))
				return;

			var view = CreateView(aWindow);
			_windows.Add(aWindow);

			var layoutAnchorable = new LayoutAnchorable();

			aWindow.WhenAnyValue(x => x.Title)
				.Subscribe(v => layoutAnchorable.Title = v);

			aWindow.WhenAnyValue(x => x.CanClose)
				.Subscribe(v =>
				{
					layoutAnchorable.CanHide = v;
					layoutAnchorable.CanClose = v;
				});

			layoutAnchorable.Content = view;

			_leftPane.Children.Add(layoutAnchorable);

		    aWindow.WhenAnyValue(x => x.WindowIsEnabled).Subscribe(v =>
		    {
		        layoutAnchorable.IsEnabled = v;
		    });

		}

		public void ShowDocument(IDockingWindow aWondow)
		{
			var view = CreateView(aWondow);
			_windows.Add(aWondow);

			var layoutDocument = new LayoutDocument();

			aWondow.WhenAnyValue(x => x.Title)
				.Subscribe(v => layoutDocument.Title = v);

			aWondow.WhenAnyValue(x => x.CanClose)
				.Subscribe(v => layoutDocument.CanClose = v);

			layoutDocument.Content = view;

			_documentPane.Children.Add(layoutDocument);

			layoutDocument.IsActive = true;
		}

		private UserControl CreateView(IDockingWindow aModel)
		{
			var types = aModel.GetType().GetInterfaces();

			Type type = null;

			foreach (var it in _modelViews.Keys)
				if (types.Contains(it))
					type = _modelViews[it];

			if (type == null)
				throw new Exception($"Model {aModel} not found");

			var view = Activator.CreateInstance(type) as UserControl;
			if (view == null)
				throw new Exception($"Cant create view for type {type}");

			view.DataContext = aModel;

			return view;
		}
	}
}
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;

namespace MonikDesktop.Oak
{
  // http://stackoverflow.com/questions/12493117/wpf-avalondock-v2-0-layoutdocument
  public class Shell : ReactiveObject
  {
    [Reactive]
    public ReactiveList<IDockingWindow> Windows { get; private set; }
    [Reactive]
    public IDockingWindow SelectedWindow { get; set; } = null;

    private DockingManager FDocker = null;
    private Dictionary<Type, Type> FModelViews;

    public Shell()
    {
      Windows = new ReactiveList<IDockingWindow>();
      FModelViews = new Dictionary<Type, Type>();
    }

    public void RegisterModelView<MODEL, VIEW>() 
      where MODEL : IDockingWindow
      where VIEW : UserControl
    {
      FModelViews.Add(typeof(MODEL), typeof(VIEW));
    }

    public void AttachDocker(DockingManager aDocker)
    {
      FDocker = aDocker;

      // TODO: not work
      FDocker.ObservableForProperty(x => x.ActiveContent)
        .Where(v => v is IDockingWindow)
        .Subscribe(v => this.SelectedWindow = v as IDockingWindow);
    }

    public void Show(IDockingWindow aWondow)
    {
      var _types = aWondow.GetType().GetInterfaces();

      Type _type = null;

      foreach (var it in FModelViews.Keys)
        if (_types.Contains(it))
          _type = FModelViews[it];

      if (_type == null)
        throw new Exception($"Model {aWondow} not found");

      var _view = Activator.CreateInstance(_type) as UserControl;
      if (_view == null)
        throw new Exception($"Cant create view for type {_type}");

      _view.DataContext = aWondow;

      var layoutdocpane = new LayoutDocumentPane();

      var LayoutDocument = new LayoutDocument();

      aWondow.WhenAnyValue(x => x.Title)
        .Subscribe(v => LayoutDocument.Title = v);

      LayoutDocument.Content = _view;

      layoutdocpane.Children.Add(LayoutDocument);
      FDocker.Layout.RootPanel.Children.Add(layoutdocpane);
      //FDocker.Layout.RootPanel.Orientation = Orientation.Vertical;
    }
  }
}

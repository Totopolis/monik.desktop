using Autofac;
using MahApps.Metro.Controls;
using MonikDesktop.Oak;
using MonikDesktop.ViewModels;
using MonikDesktop.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.AvalonDock.Layout;

namespace MonikDesktop
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : MetroWindow
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    protected override void OnInitialized(EventArgs e)
    {
      base.OnInitialized(e);

      //var _dts = new OakDataTemplateSelector();
      //var _dt = _dts.RegisterDataTemplate(typeof(StartupViewModel), typeof(StartupView));
      //MainDocker.LayoutItemTemplateSelector = _dts;
      //this.DataContext = _shell;

      Bootstrap.Init();

      var _shell = Bootstrap.Container.Resolve<Shell>();
      _shell.AttachDocker(MainDocker);

      var _startup = Bootstrap.Container.Resolve<IStartupWindow>();
      _shell.Show(_startup);
    }

    private void DockingManager_ActiveContentChanged(object sender, EventArgs e)
    {
      var _win = sender as IDockingWindow;
    }
  }

  public class OakDataTemplateSelector : DataTemplateSelector
  {
    private Dictionary<Type, DataTemplate> FTemplates = new Dictionary<Type, DataTemplate>();

    public DataTemplate RegisterDataTemplate(Type aModel, Type aView)
    {
      DataTemplate _dt = new DataTemplate();
      _dt.DataType = aModel;
      FrameworkElementFactory _fac = new FrameworkElementFactory(aView);
      _dt.VisualTree = _fac;

      FTemplates.Add(aModel, _dt);

      return _dt;
    }
    
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      var _cp = item as ContentPresenter;
      var _ctype = _cp.Content.GetType();

      _cp.Content = new Binding(".");
      //_cp.Name = "XXYY";
      //_cp.DataContext = _cp.Content;

      //container.SetValue(ContentControl.ContentProperty, new Binding());
      //container.SetValue(ContentControl.DataContextProperty, _cp.Content);

      DataTemplate _res = null;

      return FTemplates.TryGetValue(_ctype, out _res) ? _res : null;
    }
  }
}

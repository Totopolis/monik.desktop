using System.Windows.Controls;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.Views
{
    public class ViewUserControl : UserControl, IView
    {
        public ViewUserControl()
        {

        }

        public ViewUserControl(IViewModel vm)
        {
            ViewModel = vm;
            DataContext = vm;
        }

        public void Configure(UiShowOptions options)
        {
            ViewModel.Title = options.Title;
            ViewModel.CanClose = options.CanClose;
        }

        public IViewModel ViewModel { get; set; }
    }
}

using MonikDesktop.Common.Interfaces;
using MonikDesktop.ViewModels;

namespace MonikDesktop.Views
{
    /// <summary>
    ///     Interaction logic for LogsView.xaml
    /// </summary>
    public partial class KeepAliveView : ViewUserControl, IShowView
    {
        public KeepAliveView(KeepAliveViewModel vm)
            : base(vm)
        {
            InitializeComponent();
        }

        public IShowViewModel ShowViewModel => ViewModel as IShowViewModel;
    }
}
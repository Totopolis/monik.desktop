using MonikDesktop.Common.Interfaces;
using MonikDesktop.ViewModels;

namespace MonikDesktop.Views
{
    /// <summary>
    ///     Interaction logic for LogsView.xaml
    /// </summary>
    public partial class LogsView : ViewUserControl, IShowView
    {
        public LogsView(LogsViewModel vm)
            : base(vm)
        {
            InitializeComponent();
        }

        public IShowViewModel ShowViewModel => ViewModel as IShowViewModel;
    }
}
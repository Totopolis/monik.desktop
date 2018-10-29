using MonikDesktop.Common.Interfaces;
using MonikDesktop.ViewModels;

namespace MonikDesktop.Views
{
    /// <summary>
    ///     Interaction logic for LogsView.xaml
    /// </summary>
    public partial class MetricsView : ViewUserControl, IShowView
    {
        public MetricsView(MetricsViewModel vm)
            : base(vm)
        {
            InitializeComponent();
        }

        public IShowViewModel ShowViewModel => ViewModel as IShowViewModel;
    }
}
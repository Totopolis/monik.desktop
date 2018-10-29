using MonikDesktop.ViewModels;
using Ui.Wpf.Common;

namespace MonikDesktop.Views
{
    /// <summary>
    ///     Interaction logic for SourcesView.xaml
    /// </summary>
    public partial class SourcesView : ViewUserControl, IToolView
    {
        public SourcesView(SourcesViewModel vm)
            : base(vm)
        {
            InitializeComponent();
        }
    }
}
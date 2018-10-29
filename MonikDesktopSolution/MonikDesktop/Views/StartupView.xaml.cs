using MonikDesktop.ViewModels;
using Ui.Wpf.Common;

namespace MonikDesktop.Views
{
    /// <summary>
    ///     Interaction logic for StartupView.xaml
    /// </summary>
    public partial class StartupView : ViewUserControl, IToolView
    {
        public StartupView(StartupViewModel vm)
            : base(vm)
        {
            InitializeComponent();
        }
    }
}
using MonikDesktop.ViewModels;
using Ui.Wpf.Common;

namespace MonikDesktop.Views
{
    /// <summary>
    ///     Interaction logic for LogDescriptionView.xaml
    /// </summary>
    public partial class LogDescriptionView : ViewUserControl, IToolView
    {
        public LogDescriptionView(LogDescriptionViewModel vm)
            : base(vm)
        {
            InitializeComponent();
        }
    }
}
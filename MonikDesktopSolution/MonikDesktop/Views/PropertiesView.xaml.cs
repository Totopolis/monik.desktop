using MonikDesktop.ViewModels;
using Ui.Wpf.Common;

namespace MonikDesktop.Views
{
    /// <summary>
    ///     Interaction logic for PropertiesView.xaml
    /// </summary>
    public partial class PropertiesView : ViewUserControl, IToolView
    {
        public PropertiesView(PropertiesViewModel vm)
            : base(vm)
        {
            InitializeComponent();
        }
    }
}
using MonikDesktop.ViewModels;

namespace MonikDesktop.Views
{
    public partial class RemoveEntitiesView : ViewUserControl
    {
        public RemoveEntitiesView(RemoveEntitiesViewModel vm)
            : base(vm)
        {
            InitializeComponent();
        }
    }
}
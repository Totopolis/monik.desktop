using MonikDesktop.Common.Interfaces;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.Views
{
    public partial class ManageGroupsView : IManageGroupsView
    {
        public ManageGroupsView(IManageGroupsViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = viewModel;
        }

        public IViewModel ViewModel { get; set; }

        public void Configure(UiShowOptions options)
        {
            ViewModel.Title = options.Title;
        }
    }
}

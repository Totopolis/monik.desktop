using MonikDesktop.Common.Interfaces;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.Views
{
    public partial class RemoveEntitiesView : IRemoveEntitiesView
    {
        public RemoveEntitiesView(IRemoveEntitiesViewModel viewModel)
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

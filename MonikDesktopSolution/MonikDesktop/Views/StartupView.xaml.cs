using MonikDesktop.Common.Interfaces;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.Views
{
    /// <summary>
    ///     Interaction logic for StartupView.xaml
    /// </summary>
    public partial class StartupView : IStartupView
	{
		public StartupView(IStartupViewModel viewModel)
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
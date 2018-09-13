using MonikDesktop.Common.Interfaces;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.Views
{
    /// <summary>
    ///     Interaction logic for LogDescriptionView.xaml
    /// </summary>
    public partial class LogDescriptionView : ILogDescriptionView
	{
		public LogDescriptionView(ILogDescriptionViewModel viewModel)
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
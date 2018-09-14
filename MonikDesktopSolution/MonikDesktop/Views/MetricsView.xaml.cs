using MonikDesktop.Common.Interfaces;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.Views
{
    /// <summary>
    ///     Interaction logic for LogsView.xaml
    /// </summary>
    public partial class MetricsView : IMetricsView
	{
		public MetricsView(IMetricsViewModel viewModel)
		{
			InitializeComponent();

	        ViewModel = viewModel;
	        DataContext = viewModel;
	    }

	    public IViewModel ViewModel { get; set; }
	    public IShowViewModel ShowViewModel => ViewModel as IShowViewModel;

        public void Configure(UiShowOptions options)
	    {
	        ViewModel.Title = options.Title;
	    }
    }
}
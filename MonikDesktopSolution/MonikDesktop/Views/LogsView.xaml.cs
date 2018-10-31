using MonikDesktop.Common.Interfaces;
using MonikDesktop.ViewModels;
using System.Reactive;

namespace MonikDesktop.Views
{
    /// <summary>
    ///     Interaction logic for LogsView.xaml
    /// </summary>
    public partial class LogsView : ViewUserControl, IShowView
    {
        public LogsView(LogsViewModel vm)
            : base(vm)
        {
            InitializeComponent();

            vm.ScrollTo.RegisterHandler(x =>
            {
                MainGrid.ScrollIntoView(x.Input);
                x.SetOutput(Unit.Default);
            });
        }

        public IShowViewModel ShowViewModel => ViewModel as IShowViewModel;
    }
}
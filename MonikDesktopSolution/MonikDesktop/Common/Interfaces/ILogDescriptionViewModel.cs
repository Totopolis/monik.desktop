using MonikDesktop.Common.ModelsApp;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.Common.Interfaces
{
    public interface ILogDescriptionViewModel : IViewModel
    {
        LogItem SelectedItem { get; set; }
    }
}

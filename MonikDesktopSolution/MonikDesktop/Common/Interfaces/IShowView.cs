using Ui.Wpf.Common;

namespace MonikDesktop.Common.Interfaces
{
    public interface IShowView : IView
    {
        IShowViewModel ShowViewModel { get; }
    }
}

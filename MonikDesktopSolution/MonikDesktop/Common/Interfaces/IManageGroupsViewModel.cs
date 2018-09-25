using MonikDesktop.Common.ModelsApp;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.Common.Interfaces
{
    public interface IManageGroupsViewModel : IViewModel
    {
        void AddInstanceToCurrentGroup(Instance instance);
        void RemoveInstanceFromCurrentGroup(Instance instance);
    }
}

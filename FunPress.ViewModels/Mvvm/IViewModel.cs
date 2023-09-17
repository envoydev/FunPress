using FunPress.Views.Mvvm;
using System.Threading.Tasks;

namespace FunPress.ViewModels.Mvvm
{
    public interface IViewModel
    {
        void AssignView(IManageView manageView);

        Task InitializeDataAsync(object param = null);

        void ClearData();
    }
}

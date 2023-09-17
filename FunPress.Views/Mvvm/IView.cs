using System.Threading.Tasks;
using FunPress.Views.Mvvm.Parameters;

namespace FunPress.Views.Mvvm
{
    public interface IView : IManageView
    {
        Task ShowViewAsync(CreateViewParameters param = null);
    }
}

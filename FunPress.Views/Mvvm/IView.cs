using System.Threading.Tasks;
using FunPress.Views.Mvvm.Parameters;

namespace FunPress.Views.Mvvm
{
    public interface IView : IManageView
    {
        // ReSharper disable once UnusedParameter.Global
        Task ShowViewAsync(CreateViewParameters param = null);
    }
}

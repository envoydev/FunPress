using FunPress.Views.Mvvm.Parameters;
using System.Threading.Tasks;

namespace FunPress.Views.Mvvm
{
    public interface IManageView
    {
        Task CloseAsync(CloseViewParameters param = null);
    }
}

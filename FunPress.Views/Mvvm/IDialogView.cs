using FunPress.Views.Mvvm.Parameters;
using System.Threading.Tasks;

namespace FunPress.Views.Mvvm
{
    public interface IDialogView : IManageView
    {
        // ReSharper disable once UnusedMemberInSuper.Global
        object DialogViewResult { get; }

        Task<bool> ShowDialogViewAsync(CreateViewParameters param = null);
    }
}

using FunPress.Views.Mvvm;

namespace FunPress.Views.Factory
{
    public interface IViewFactory
    {
        T Get<T>() where T: IManageView;
    }
}

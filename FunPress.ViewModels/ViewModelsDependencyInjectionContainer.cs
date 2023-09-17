using FunPress.ViewModels.Contracts;
using FunPress.ViewModels.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace FunPress.ViewModels
{
    public static class ViewModelsDependencyInjectionContainer
    {
        public static void Register(IServiceCollection services)
        {
            services.AddTransient<IFunPressViewModel, FunPressViewModel>();
            services.AddTransient<IMessageDialogViewModel, MessageDialogViewModel>();
        }
    }
}

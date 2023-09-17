using FunPress.Views.Factory;
using FunPress.Startup.Views;
using FunPress.Startup.Views.Common;
using FunPress.Views.DialogViews;
using FunPress.Views.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FunPress.Startup.Factory
{
    public static class ViewsDependencyInjectionContainer
    {
        public static void Register(IServiceCollection services)
        {
            services.AddTransient<IViewFactory, ViewFactory>();

            services.AddTransient<App>();

            services.AddTransient<IFunPressView, FunPressView>();
            services.AddTransient<IMessageDialogView, MessageDialogView>();
        }
    }
}

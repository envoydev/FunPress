using FunPress.Startup.Factory;
using FunPress.Startup.Views;
using FunPress.Startup.Views.Common;
using FunPress.Views.DialogViews;
using FunPress.Views.Factory;
using FunPress.Views.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FunPress.Startup
{
    public static class StartupDependencyInjectionContainer
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

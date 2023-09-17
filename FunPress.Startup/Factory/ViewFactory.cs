using System;
using FunPress.Views.Factory;
using FunPress.Views.Mvvm;
using Microsoft.Extensions.DependencyInjection;

namespace FunPress.Startup.Factory
{
    internal class ViewFactory : IViewFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ViewFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T Get<T>() where T : IManageView
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}

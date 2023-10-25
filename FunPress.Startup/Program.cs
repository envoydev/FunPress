using System;
using System.Diagnostics;
using System.Threading;
using FunPress.Common.Constants;
using FunPress.Core;
using FunPress.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FunPress.Startup
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            var mutex = new Mutex(true, ApplicationConstants.ApplicationName, out var createdNew);
            if (!createdNew)
            {
                return;
            }

            try
            {
                var services = new ServiceCollection();

                CoreDependencyInjectionContainer.Register(services);
                StartupDependencyInjectionContainer.Register(services);
                ViewModelsDependencyInjectionContainer.Register(services);

                var serviceProvider = services.BuildServiceProvider();

                var app = serviceProvider.GetRequiredService<App>();
                app.InitializeComponent();
                app.Run();

                serviceProvider.Dispose();
            }
            catch (Exception exception)
            {
                var eventLog = new EventLog
                {
                    Source = ApplicationConstants.ApplicationName, 
                    Log = "Application"
                };

                var message = $"{exception.Message}{Environment.NewLine}{Environment.NewLine}{exception.StackTrace}";
                eventLog.WriteEntry(message, EventLogEntryType.Error);
            }
            finally
            {
                mutex.Dispose();
            }
        }
    }
}

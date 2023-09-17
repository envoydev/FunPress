using FunPress.Core.Services.Implementations;
using FunPress.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using FunPress.Core.Logger;

namespace FunPress.Core
{
    public static class CoreDependencyInjectionContainer
    {
        public static void Register(IServiceCollection services)
        {
            services.AddSingleton<IApplicationEnvironment, ApplicationEnvironment>();
            services.AddSingleton<IApplicationService, ApplicationService>();
            services.AddSingleton<ISerializeService, SerializeService>();
            services.AddSingleton<IJobService, JobService>();
            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddTransient<IPrinterService, PrinterService>();
            services.AddTransient<IImageService, ImageService>();
            services.AddTransient<IDelayService, DelayService>();

            services.AddLogging(loggingFactory =>
            {
                loggingFactory.AddApplicationSerilog();
            });
        }
    }
}

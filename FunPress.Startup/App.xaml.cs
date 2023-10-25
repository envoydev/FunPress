using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using FunPress.Common.Types.Enums;
using FunPress.Core.Services;
using FunPress.Views.DialogViews;
using FunPress.Views.Factory;
using FunPress.Views.Mvvm.Parameters;
using FunPress.Views.Params;
using FunPress.Views.Views;
using Microsoft.Extensions.Logging;

namespace FunPress.Startup
{
    public partial class App
    {
        private readonly ILogger<App> _logger;
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly IApplicationService _applicationService;
        private readonly IJobService _jobService;
        private readonly ILanguageService _languageService;
        private readonly IViewFactory _viewFactory;

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static IServiceProvider ServiceProvider { get; private set; }

        public App(
            ILogger<App> logger, 
            IServiceProvider serviceProvider, 
            IApplicationEnvironment applicationEnvironment,
            IApplicationService applicationService,
            IJobService jobService,
            ILanguageService languageService,
            IViewFactory viewFactory
            )
        {
            _logger = logger;
            _applicationEnvironment = applicationEnvironment;
            _applicationService = applicationService;
            _languageService = languageService;
            _jobService = jobService;
            _viewFactory = viewFactory;

            ServiceProvider = serviceProvider;
        }

        protected override void OnStartup(StartupEventArgs args)
        {
            _languageService.SetDefaultLanguage();
            
            SetupUnhandledExceptionHandling();

            base.OnStartup(args);
        }

        // Entry point after OnStartup
        protected void EntryPoint(object sender, StartupEventArgs args)
        {
            Task.Run(async () =>
            {
                try
                {
                    _logger.LogInformation("@ <Starting v{ApplicationVersion}> @",
                        _applicationEnvironment.GetApplicationVersion());
                    
                    _logger.LogInformation("Configuration: {Configuration}",
                        _applicationEnvironment.GetConfigurationType());
                    
                    _logger.LogInformation("Base path: {Configuration}",
                        _applicationEnvironment.GetApplicationBasePath());

                    await _applicationService.DispatcherInvokeAsync(async () =>
                    {
                        await _viewFactory.Get<IFunPressView>().ShowViewAsync();
                    });
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Invoke in {Method}", nameof(EntryPoint));

                    await ShowErrorMessageAsync();

                    _applicationService.ApplicationShutdown();
                }
            });
        }

        protected override void OnExit(ExitEventArgs args)
        {
            try
            {
                _logger.LogInformation("Invoke in {Method}. Start exit process",
                    nameof(OnExit));

                _applicationService.DispatcherInvoke(() =>
                {
                    _applicationService.CloseAllWindows();
                });

                _jobService.FinishAllJobs();

                _logger.LogInformation("@ <Finish v{ApplicationVersion}> @",
                    _applicationEnvironment.GetApplicationVersion());
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(OnExit));
            }

            base.OnExit(args);
        }

        #region Private methods

        private void SetupUnhandledExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                LogUnhandledException((Exception)args.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");
            };

            DispatcherUnhandledException += (_, args) =>
            {
                LogUnhandledException(args.Exception, "Application.Current.DispatcherUnhandledException");
                args.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                LogUnhandledException(args.Exception, "TaskScheduler.UnobservedTaskException");
                args.SetObserved();
            };
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            try
            {
                var assemblyName = Assembly.GetExecutingAssembly().GetName();

                _logger.LogError("Invoke in {Method}. Message: {Message}", nameof(LogUnhandledException),
                    $"Unhandled exception in {assemblyName.Name} v{assemblyName.Version}");
            }
            catch (Exception currentException)
            {
                _logger.LogError(currentException, "Invoke in {Method}", nameof(LogUnhandledException));
            }
            finally
            {
                _logger.LogError(exception, "Invoke in {Method}. Message: {Message}",
                    nameof(LogUnhandledException), $"Unhandled exception ({source})");
            }
        }

        private async Task ShowErrorMessageAsync()
        {
            const string message = "There is unexpected error. Application will shutdown. Please, see logs for more details.";

            var viewParameters = new CreateViewParameters
            {
                AdditionalParameters = new MessageDialogParam(message,
                    new[] { MessageDialogButton.Ok })
            };

            await _viewFactory.Get<IMessageDialogView>()
                .ShowDialogViewAsync(viewParameters);
        }

        #endregion
    }
}

using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FunPress.Core.Services.Implementations
{
    internal class ApplicationService : IApplicationService
    {
        private readonly ILogger<ApplicationService> _logger;

        public ApplicationService(ILogger<ApplicationService> logger) 
        {
            _logger = logger;
        }

        public void ApplicationShutdown()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    _logger.LogInformation("Invoke in {Method}", nameof(ApplicationShutdown));

                    Application.Current.Shutdown();
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Invoke in {Method}", nameof(ApplicationShutdown));
                }

            }, DispatcherPriority.Send);
        }

        public Window[] GetAllWindows()
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    _logger.LogInformation("Invoke in {Method}", nameof(GetAllWindows));

                    var array = new Window[Application.Current.Windows.Count];

                    Application.Current.Windows.CopyTo(array, 0);

                    return array;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Invoke in {Method}", nameof(GetAllWindows));

                    return Array.Empty<Window>();
                }

            }, DispatcherPriority.Send);
        }

        public void CloseAllWindows()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    _logger.LogInformation("Invoke in {Method}. Start closing all windows. Windows to close: {Count}", 
                        nameof(CloseAllWindows), Application.Current.Windows.Count);

                    foreach (Window window in Application.Current.Windows)
                    {
                        window.Close();
                    }

                    _logger.LogInformation("Invoke in {Method}. Finish closing windows", 
                        nameof(CloseAllWindows));
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Invoke in {Method}", nameof(CloseAllWindows));
                }

            }, DispatcherPriority.Send);
        }

        public void SetMainWindow(object window)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    _logger.LogInformation("Invoke in {Method}", nameof(SetMainWindow));

                    Application.Current.MainWindow = (Window)window;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Invoke in {Method}", nameof(SetMainWindow));
                }

            }, DispatcherPriority.Send);
        }

        public object GetMainWindow()
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    _logger.LogInformation("Invoke in {Method}", nameof(GetMainWindow));

                    return Application.Current.MainWindow;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Invoke in {Method}", nameof(GetMainWindow));

                    return null;
                }

            }, DispatcherPriority.Send);
        }

        public void DispatcherInvoke(Action action, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken cancellationToken = default)
        {
            Exception exception = null;

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    action();
                }
                catch (Exception possibleException)
                {
                    exception = possibleException;
                }

            }, priority, cancellationToken);

            if (exception != null)
            {
                throw exception;
            }
        }

        public T DispatcherInvoke<T>(Func<T> func, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken cancellationToken = default)
        {
            Exception exception = null;
            T result = default;

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    result = func();
                }
                catch (Exception possibleException)
                {
                    exception = possibleException;
                }

            }, priority, cancellationToken);

            if (exception != null)
            {
                throw exception;
            }

            return result;
        }

        public async Task DispatcherInvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken cancellationToken = default)
        {
            Exception exception = null;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    action();
                }
                catch (Exception possibleException)
                {
                    exception = possibleException;
                }

            }, priority, cancellationToken);

            if (exception != null)
            {
                throw exception;
            }
        }

        public async Task<T> DispatcherInvokeAsync<T>(Func<T> func, DispatcherPriority priority = DispatcherPriority.Normal, 
            CancellationToken cancellationToken = default)
        {
            Exception exception = null;
            T result = default;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    result = func();
                }
                catch (Exception possibleException)
                {
                    exception = possibleException;
                }

            }, priority, cancellationToken);

            if (exception != null)
            {
                throw exception;
            }

            return result;
        }

        public async Task<T> DispatcherInvokeAsync<T>(Func<Task<T>> func, DispatcherPriority priority = DispatcherPriority.Normal, 
            CancellationToken cancellationToken = default)
        {
            Exception exception = null;
            T result = default;

            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    result = await func();
                }
                catch (Exception possibleException)
                {
                    exception = possibleException;
                }

            }, priority, cancellationToken);

            if (exception != null)
            {
                throw exception;
            }

            return result;
        }

        public void DispatcherInvokeAsAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal,
            CancellationToken cancellationToken = default)
        {
            Application.Current.Dispatcher.InvokeAsync(action, priority, cancellationToken);
        }

        public void DispatcherInvokeAsAsync(Func<Task> func, DispatcherPriority priority = DispatcherPriority.Normal,
            CancellationToken cancellationToken = default)
        {
            Application.Current.Dispatcher.InvokeAsync(func, priority, cancellationToken);
        }
    }
}

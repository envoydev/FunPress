using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FunPress.Core.Services
{
    public interface IApplicationService
    {
        // ReSharper disable UnusedMember.Global
        void ApplicationShutdown();
        Window[] GetAllWindows();
        void CloseAllWindows();
        void SetMainWindow(object window);
        object GetMainWindow();
        void DispatcherInvoke(Action action, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken cancellationToken = default);
        T DispatcherInvoke<T>(Func<T> func, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken cancellationToken = default);
        void DispatcherInvokeAsAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken cancellationToken = default);
        void DispatcherInvokeAsAsync(Func<Task> func, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken cancellationToken = default);
        Task DispatcherInvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken cancellationToken = default);
        Task<T> DispatcherInvokeAsync<T>(Func<T> func, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken cancellationToken = default);
        Task<T> DispatcherInvokeAsync<T>(Func<Task<T>> func, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken cancellationToken = default);
        // ReSharper restore UnusedMember.Global
    }
}

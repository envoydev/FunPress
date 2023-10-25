using System;
using System.Threading;
using System.Threading.Tasks;

namespace FunPress.Core.Services
{
    public interface IDelayService
    {
        // ReSharper disable UnusedMember.Global
        Task WaitForConditionAsync(Func<bool> condition, TimeSpan pollingInterval, CancellationToken cancellationToken);
        // ReSharper disable once UnusedMemberInSuper.Global
        Task WaitForConditionAsync(Func<bool> condition, int intervalMilliseconds, CancellationToken cancellationToken);
        Task DelayAsync(TimeSpan timeSpan, CancellationToken cancellationToken);
        Task DelayAsync(int milliseconds, CancellationToken cancellationToken);
        Task DelayAsync(double milliseconds, CancellationToken cancellationToken);
        // ReSharper restore UnusedMember.Global
    }
}

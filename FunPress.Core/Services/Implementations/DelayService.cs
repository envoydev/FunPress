using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FunPress.Core.Services.Implementations
{
    internal class DelayService : IDelayService
    {
        private readonly ILogger<DelayService> _logger;

        public DelayService(ILogger<DelayService> logger)
        {
            _logger = logger;
        }

        public async Task WaitForConditionAsync(Func<bool> condition, TimeSpan pollingInterval, CancellationToken cancellationToken)
        {
            try
            {
                while (condition())
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(cancellationToken);
                    }

                    await DelayAsync(pollingInterval, cancellationToken);
                }
            }
            catch (OperationCanceledException operationCanceledException)
            {
                _logger.LogDebug("Invoke in {Method}. Message: {Message}",
                    nameof(WaitForConditionAsync), operationCanceledException.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(WaitForConditionAsync));
            }
        }

        public async Task WaitForConditionAsync(Func<bool> condition, int pollingIntervalMilliseconds, CancellationToken cancellationToken)
        {
            await WaitForConditionAsync(condition, TimeSpan.FromMilliseconds(pollingIntervalMilliseconds), cancellationToken);
        }

        public async Task DelayAsync(int milliseconds, CancellationToken cancellationToken)
        {
            await DelayAsync(TimeSpan.FromMilliseconds(milliseconds), cancellationToken);
        }

        public async Task DelayAsync(double milliseconds, CancellationToken cancellationToken)
        {
            await DelayAsync(TimeSpan.FromMilliseconds(milliseconds), cancellationToken);
        }

        public async Task DelayAsync(TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            await Task.Delay(timeSpan, cancellationToken);
        }
    }
}

using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using FunPress.Core.Services;

namespace FunPress.Core.Containers
{
    internal class JobContainer
    {
        private readonly ILogger<JobContainer> _logger;
        private readonly IDelayService _delayService;

        private readonly Func<CancellationToken, Task> _funcToRun;
        private readonly TimeSpan _interval;

        private CancellationTokenSource _jobCancellationTokenSource;

        public string Key { get; }

        public JobContainer(string key, TimeSpan interval, Func<CancellationToken, Task> funcToRun, 
            ILogger<JobContainer> logger, IDelayService delayService)
        {
            Key = key;
            _interval = interval;
            _funcToRun = funcToRun;
            _logger = logger;
            _delayService = delayService;
        }

        public void Start(bool isStartImmediately)
        {
            _jobCancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    if (!isStartImmediately)
                    {
                        await _delayService.DelayAsync(_interval, _jobCancellationTokenSource.Token);
                    }

                    while (true)
                    {
                        if (_jobCancellationTokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                    
                        await _funcToRun(_jobCancellationTokenSource.Token);
                
                        await _delayService.DelayAsync(_interval, _jobCancellationTokenSource.Token);
                    }
                }
                catch (OperationCanceledException operationCanceledException)
                {
                    _logger.LogTrace("Invoke in {Method}. Message: {Message}", 
                        nameof(Start), operationCanceledException.Message);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Error in job task with key: {Key}", Key);
                }
            });
        }

        public void Stop()
        {
            _jobCancellationTokenSource.Cancel();
        }
    }
}

using FunPress.Core.Containers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FunPress.Core.Services.Implementations
{
    internal class JobService : IJobService
    {
        private readonly ILogger<JobService> _logger;
        private readonly ILogger<JobContainer> _jobContainerLogger;
        private readonly IDelayService _delayService;

        private ConcurrentDictionary<string, JobContainer> _jobs;

        public JobService(
            ILoggerFactory loggerFactory,
            IDelayService delayService
            )
        {
            _logger = loggerFactory.CreateLogger<JobService>();
            _jobContainerLogger = loggerFactory.CreateLogger<JobContainer>();
            _delayService = delayService;

            _jobs = new ConcurrentDictionary<string, JobContainer>();
        }

        public bool IsJobExist(string key)
        {
            try
            {
                return _jobs.ContainsKey(key);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}. Key {Key}",
                    nameof(CreateJob), key);

                return false;
            }
        }

        public bool CreateJob(string key, Func<CancellationToken, Task> funcToRun)
        {
            return CreateJob(key, TimeSpan.Zero, funcToRun);
        }

        public bool CreateJob(string key, TimeSpan interval, Func<CancellationToken, Task> funcToRun)
        {
            try
            {
                if (_jobs.ContainsKey(key))
                {
                    _logger.LogDebug("Invoke in {Method}. {Key} is exist in jobs collection",
                        nameof(CreateJob), key);

                    return false;
                }

                var job = new JobContainer(key, interval, funcToRun, _jobContainerLogger, _delayService);

                var result = _jobs.TryAdd(key, job);

                if (!result)
                {
                    _logger.LogWarning("Invoke in {Method}. Job with {Key} did not added",
                        nameof(FinishAllJobs), key);

                    return false;
                }

                _logger.LogInformation("Invoke in {Method}. Job with key {Key} and time interval '{Interval}' created",
                    nameof(CreateJob), key, interval);

                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}. Key {Key}. Interval {Interval}",
                    nameof(CreateJob), key, interval);

                return false;
            }
        }

        public bool StartJob(string key, bool isStartImmediately)
        {
            try
            {
                if (_jobs.All(x => x.Key != key))
                {
                    _logger.LogDebug("Invoke in {Method}. {Key} does not exist in jobs collection",
                        nameof(StartJob), key);

                    return false;
                }

                var result = _jobs.TryGetValue(key, out var job);

                if (!result)
                {
                    _logger.LogWarning("Invoke in {Method}. Job with {Key} did not started",
                        nameof(StartJob), key);

                    return false;
                }

                job.Start(isStartImmediately);

                _logger.LogInformation("Invoke in {Method}. Job with key {Key} started",
                    nameof(StartJob), key);

                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(StartJob));

                return false;
            }
        }

        public bool FinishJob(string key)
        {
            try
            {
                if (!_jobs.ContainsKey(key))
                {
                    _logger.LogDebug("Invoke in {Method}. {Key} does not exist in jobs collection",
                        nameof(FinishJob), key);

                    return false;
                }

                var isJobRemoved = _jobs.TryRemove(key, out var job);
                if (!isJobRemoved)
                {
                    _logger.LogWarning("Invoke in {Method}. Job with {Key} did not removed",
                        nameof(FinishJob), key);

                    return false;
                }

                job.Stop();

                _logger.LogInformation("Invoke in {Method}. Job with key {Key} is removed",
                     nameof(FinishJob), key);

                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(FinishJob));

                return false;
            }
        }

        public bool FinishAllJobs()
        {
            try
            {
                if (!_jobs.Any())
                {
                    _logger.LogDebug("Invoke in {Method}. There is no any jobs registered",
                        nameof(FinishAllJobs));

                    return false;
                }

                foreach (var jobKeyValue in _jobs)
                {
                    jobKeyValue.Value.Stop();
                }

                _jobs.Clear();
                _jobs = new ConcurrentDictionary<string, JobContainer>();

                _logger.LogInformation("Invoke in {Method}. All jobs removed",
                    nameof(FinishJob));

                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(FinishAllJobs));

                return false;
            }
        }
    }
}

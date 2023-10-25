using System;
using System.Threading;
using System.Threading.Tasks;

namespace FunPress.Core.Services
{
    public interface IJobService
    {
        bool IsJobExist(string key);
        // ReSharper disable once UnusedMemberInSuper.Global
        bool CreateJob(string key, Func<CancellationToken, Task> funcToRun);
        bool CreateJob(string key, TimeSpan interval, Func<CancellationToken, Task> funcToRun);
        bool StartJob(string key, bool isStartImmediately);
        bool FinishJob(string key);
        bool FinishAllJobs();
    }
}

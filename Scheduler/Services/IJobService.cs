using Scheduler.Interfaces;
using System;

namespace Scheduler.Services
{
    public interface IJobService
    {
        void Execute<TJob>(TJob job) where TJob : class, IJob;

        void ExecuteWithDelay<TJob>(TJob job, TimeSpan time) where TJob : class, IJob;
    }
}

using Scheduler.Interfaces;
using System;

namespace Scheduler.Services
{
    public interface IJobService
    {
        string FireAndForgotService<TJob>(TJob job) where TJob : class, IJob;

        string DelayedJobsService<TJob>(TJob job, TimeSpan time) where TJob : class, IJob;
        
    }
}

using Hangfire;
using Scheduler.Interfaces;
using System;

namespace Scheduler.Services
{
    public class JobService : IJobService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IServiceProvider _serviceProvider;

        public JobService(IBackgroundJobClient backgroundJobClient,
            IServiceProvider serviceProvider)
        {
            _backgroundJobClient = backgroundJobClient;
            _serviceProvider = serviceProvider;
        }

        public string FireAndForgotService<TJob>(TJob job)
            where TJob : class, IJob
        {
            var type = typeof(IJobExecutor<>).MakeGenericType(typeof(TJob));
            var jobExecutorInstance = _serviceProvider.GetService(type) as IJobExecutor<TJob>;
            return _backgroundJobClient.Enqueue(() => jobExecutorInstance.Execute(job));
        }

        public string DelayedJobsService<TJob>(TJob job, TimeSpan time)
            where TJob : class, IJob
        {
            var type = typeof(IJobExecutor<>).MakeGenericType(typeof(TJob));
            var jobExecutorInstance = _serviceProvider.GetService(type) as IJobExecutor<TJob>;
            return _backgroundJobClient.Schedule(() => jobExecutorInstance.Execute(job), time);
        }
       
    }
}

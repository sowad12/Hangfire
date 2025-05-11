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

        public void Execute<TJob>(TJob job)
            where TJob : class, IJob
        {
            var type = typeof(IJobExecutor<>).MakeGenericType(typeof(TJob));
            var jobExecutorInstance = _serviceProvider.GetService(type) as IJobExecutor<TJob>;
            _backgroundJobClient.Enqueue(() => jobExecutorInstance.Execute(job));
        }

        public void ExecuteWithDelay<TJob>(TJob job, TimeSpan time)
            where TJob : class, IJob
        {
            var type = typeof(IJobExecutor<>).MakeGenericType(typeof(TJob));
            var jobExecutorInstance = _serviceProvider.GetService(type) as IJobExecutor<TJob>;
            _backgroundJobClient.Schedule(() => jobExecutorInstance.Execute(job), time);
        }


    }
}

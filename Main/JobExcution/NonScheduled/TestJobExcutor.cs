using Main.Models.Jobs;
using Scheduler.Interfaces;

namespace Main.JobExcution.NonScheduled
{
    public class TestJobExcutor : IJobExecutor<SampleJob>
    {
        public Task Execute(SampleJob job)
        {
            throw new NotImplementedException();
        }
    }
}

using Main.Models.Jobs;
using Scheduler.Interfaces;

namespace Main.JobExcution.NonScheduled
{
    public class DelayedJobExcutor : IJobExecutor<DelayedJob>
    {
        public DelayedJobExcutor()
        {
            
        }
        public async Task Execute(DelayedJob job)
        {
            try
            {
                Console.Write(job.Name);
            }
            catch (Exception ex)
            {
                await Task.CompletedTask;
            }
        }
    }   
    
}

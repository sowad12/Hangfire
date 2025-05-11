using Main.Models.Jobs;
using Scheduler.Interfaces;

namespace Main.JobExcution.NonScheduled
{
    public class FireAndForgotJobExcutor : IJobExecutor<FireAndForgotJob>
    {
        public async Task Execute(FireAndForgotJob job)
        {
            try
            {
                Console.Write(job.Name);
            }catch(Exception ex)
            {
                await Task.CompletedTask;
            }
      
        }
    }
}

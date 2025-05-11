using Scheduler.Interfaces;

namespace Main.JobExcution.Scheduled
{
    public class RecurringJobExecutor : IScheduledJobExecutor
    {
    

        public RecurringJobExecutor()
        {
          
        }

        public async Task Execute()
        {
            await Task.Delay(20);

            Console.WriteLine("The method will be executed in the associated service...\n");
        }

        public string GetCron()
        {
            return $"*/{2} * * * *"; // Run after every 2 minutes
        }

        public string GetUniqueJobId()
        {
            return nameof(RecurringJobExecutor);
        }
    }
}

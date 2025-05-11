using System.Threading.Tasks;

namespace Scheduler.Interfaces
{
    public interface IScheduledJobExecutor
    {
        Task Execute();

        string GetUniqueJobId();

        string GetCron();
    }
}


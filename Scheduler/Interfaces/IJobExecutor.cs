using System.Threading.Tasks;

namespace Scheduler.Interfaces
{
    public interface IJobExecutor<TJob> where TJob : IJob
    {
        Task Execute(TJob job);

    }
}


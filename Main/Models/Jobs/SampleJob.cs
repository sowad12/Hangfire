using Scheduler.Interfaces;

namespace Main.Models.Jobs
{
    public class SampleJob:IJob
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}

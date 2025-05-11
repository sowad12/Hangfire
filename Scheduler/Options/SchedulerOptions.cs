namespace Scheduler.Options
{
    public class SchedulerOptions
    {
        public bool Enabled { get; set; }
        public string Dashboard { get; set; }
        public bool SecurityEnabled { get; set; }
        public string StorageType { get; set; }
        public SchedulerStorageOptions StorageOptions { get; set; }
    }
}

namespace Rangeman.Services.BackgroundTimeSyncService
{
    public interface ITimeSyncServiceStatus
    {
        bool IsStarted { get; set; }
    }
}
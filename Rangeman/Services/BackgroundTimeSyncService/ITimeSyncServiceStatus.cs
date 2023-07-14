namespace Rangeman.Services.BackgroundTimeSyncService
{
    public interface ITimeSyncServiceStatus
    {
        TimeSyncServiceState GetState();

        void SetState(TimeSyncServiceState state);
    }
}
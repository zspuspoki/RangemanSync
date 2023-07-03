using Rangeman.Services.BackgroundTimeSyncService;

namespace RangemanSync.Android.Services.BackgroundTimeSync
{
    public class TimeSyncServiceStatus : ITimeSyncServiceStatus
    {
        public bool IsStarted { get ; set; }
    }
}
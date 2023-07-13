using Android.OS;

namespace RangemanSync.Android.Services.BackgroundTimeSync
{
    public class BackgroundTimeSyncServiceBinder : Binder
    {
        private readonly BackgroundTimeSyncService service;

        public BackgroundTimeSyncServiceBinder(BackgroundTimeSyncService service)
        {
            this.service = service;
        }

        public BackgroundTimeSyncService GetBackgroundTimeSyncService()
        {
            return service;
        }
    }
}
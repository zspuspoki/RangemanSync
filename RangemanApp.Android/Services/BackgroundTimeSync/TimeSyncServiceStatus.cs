using Rangeman.Services.BackgroundTimeSyncService;
using Xamarin.Forms;

namespace RangemanSync.Android.Services.BackgroundTimeSync
{
    public class TimeSyncServiceStatus : ITimeSyncServiceStatus
    {
        private TimeSyncServiceState state;

        public TimeSyncServiceStatus()
        {
        }

        public TimeSyncServiceState GetState()
        {
            return state;
        }

        public void SetState(TimeSyncServiceState state)
        {
            MessagingCenter.Send<ITimeSyncServiceStatus>(this, TimeSyncServiceMessages.ServiceStateChanged.ToString());
            this.state = state;
        }
    }
}
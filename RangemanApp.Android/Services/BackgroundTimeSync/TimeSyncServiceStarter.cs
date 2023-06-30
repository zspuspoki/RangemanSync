using Android.Content;
using Rangeman.Services.BackgroundTimeSyncService;
using Xamarin.Forms.Platform.Android;

namespace RangemanSync.Android.Services.BackgroundTimeSync
{
    public class TimeSyncServiceStarter : ITimeSyncServiceStarter
    {
        private readonly FormsAppCompatActivity mainActivity;
        private Intent startServiceIntent;
        private Intent stopServiceIntent;

        public TimeSyncServiceStarter(FormsAppCompatActivity mainActivity)
        {
            this.mainActivity = mainActivity;

            startServiceIntent = new Intent(mainActivity, typeof(BackgroundTimeSyncService));
            startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);

            stopServiceIntent = new Intent(mainActivity, typeof(BackgroundTimeSyncService));
            stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);
        }

        public void Start()
        {
            mainActivity.StartService(startServiceIntent);
        }

        public void Stop()
        {
            mainActivity.StopService(stopServiceIntent);
        }
    }
}
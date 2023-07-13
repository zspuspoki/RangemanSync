using Android.App;
using Android.Content;
using Android.OS;
using Rangeman;
using Rangeman.Services.BackgroundTimeSyncService;
using Xamarin.Forms.Platform.Android;

namespace RangemanSync.Android.Services.BackgroundTimeSync
{
    public class TimeSyncServiceStatus : ITimeSyncServiceStatus
    {
        private readonly FormsAppCompatActivity mainActivity;

        public TimeSyncServiceStatus()
        {
            var appShell = ((AppShell)App.Current.MainPage);

            this.mainActivity =
                (FormsAppCompatActivity)appShell.ServiceProvider.GetService(typeof(FormsAppCompatActivity));
        }

        public bool IsRunning()
        {
            var alarmIntent = new Intent(BackgroundTimeSyncService.AlarmIntentName);

            var alreadyUsedPendingIntent = (Build.VERSION.SdkInt >= BuildVersionCodes.M) ?
                PendingIntent.GetBroadcast(mainActivity, 0, alarmIntent, PendingIntentFlags.NoCreate | PendingIntentFlags.Immutable) :
                PendingIntent.GetBroadcast(mainActivity, 0, alarmIntent, PendingIntentFlags.NoCreate);

            if (alreadyUsedPendingIntent != null)
            {
                return true;
            }

            return false;
        }
    }
}
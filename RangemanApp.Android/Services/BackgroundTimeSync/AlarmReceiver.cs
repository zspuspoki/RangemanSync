using Android.App;
using Android.Content;
using Android.OS;
using Rangeman;
using Xamarin.Forms.Platform.Android;
using static Android.OS.PowerManager;

namespace RangemanSync.Android.Services.BackgroundTimeSync
{
    [BroadcastReceiver]
    public class AlarmReceiver : BroadcastReceiver
    {
        private static WakeLock wakeLock;
        private readonly FormsAppCompatActivity mainActivity;

        public AlarmReceiver()
        {
            var appShell = ((AppShell)App.Current.MainPage);

            this.mainActivity =
                (FormsAppCompatActivity)appShell.ServiceProvider.GetService(typeof(FormsAppCompatActivity));
        }

        public override void OnReceive(Context context, Intent intent)
        {
            //TODO: acquire wake lock
            //start service

            PowerManager powerManager = Application.Context.GetSystemService(Context.PowerService) as PowerManager;
            wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "ServiceWakeLock");
            wakeLock.SetReferenceCounted(false);

            if (wakeLock != null)
                wakeLock.Acquire();

            var ntpServer = intent.GetStringExtra(Constants.START_SERVICE_NTP_SERVER);
            var compensationSeconds = intent.Extras.GetDouble(Constants.START_SERVICE_COMPENSATION_SECONDS);

            var startServiceIntent = new Intent(mainActivity, typeof(BackgroundTimeSyncService));
            startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);

            startServiceIntent.PutExtra(Constants.START_SERVICE_COMPENSATION_SECONDS, compensationSeconds);
            startServiceIntent.PutExtra(Constants.START_SERVICE_NTP_SERVER, ntpServer);
            mainActivity.StartService(startServiceIntent);
        }

        public static void ReleaseWakeLock()
        {
            if (wakeLock != null)
            {
                wakeLock.Release();
                wakeLock = null;
            }
        }
    }
}
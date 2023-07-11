using Android.App;
using Android.Content;
using Android.OS;
using Microsoft.Extensions.Logging;
using Rangeman;
using Rangeman.Services.BackgroundTimeSyncService;
using Serilog.Context;
using Xamarin.Forms.Platform.Android;

namespace RangemanSync.Android.Services.BackgroundTimeSync
{
    public class TimeSyncServiceStarter : ITimeSyncServiceStarter
    {
        private readonly FormsAppCompatActivity mainActivity;
        private Intent startServiceIntent;
        private ILoggerFactory loggerFactory;
        private ILogger<TimeSyncServiceStarter> logger;

        public TimeSyncServiceStarter()
        {
            var appShell = ((AppShell)App.Current.MainPage);

            this.mainActivity =
                (FormsAppCompatActivity)appShell.ServiceProvider.GetService(typeof(FormsAppCompatActivity));

            this.loggerFactory =
              (ILoggerFactory)appShell.ServiceProvider.GetService(typeof(ILoggerFactory));

            this.logger = loggerFactory.CreateLogger<TimeSyncServiceStarter>();

            startServiceIntent = new Intent(mainActivity, typeof(BackgroundTimeSyncService));
            startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);
        }

        public void Start(string ntpServer, double compensationSeconds)
        {
            var alarmIntent = new Intent(mainActivity, typeof(AlarmReceiver));
            alarmIntent.PutExtra(Constants.START_SERVICE_COMPENSATION_SECONDS, compensationSeconds);
            alarmIntent.PutExtra(Constants.START_SERVICE_NTP_SERVER, ntpServer);

            var alarmManager = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService);

            CancelAlreadyScheduledAlarm(alarmIntent, alarmManager);

            var pending = (Build.VERSION.SdkInt >= BuildVersionCodes.M) ?
                PendingIntent.GetBroadcast(mainActivity, 0, alarmIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable) :
                PendingIntent.GetBroadcast(mainActivity, 0, alarmIntent, PendingIntentFlags.UpdateCurrent);

            using (LogContext.PushProperty("BackgroundTimeSyncService", 1))
            {
                try
                {
                    logger.LogDebug("Scheduling next time sync ...");

                    var timeSyncScheduler = new TimeSyncScheduler();
                    alarmManager.SetExactAndAllowWhileIdle(AlarmType.ElapsedRealtime, timeSyncScheduler.GetTriggerMilis(), pending);
                }
                catch
                {
                    logger.LogDebug("An unexpected error occured during scheduling next sync");
                }
            }
        }

        private void CancelAlreadyScheduledAlarm(Intent alarmIntent, AlarmManager alarmManager)
        {
            var alreadyUsedPendingIntent = (Build.VERSION.SdkInt >= BuildVersionCodes.M) ?
                PendingIntent.GetBroadcast(mainActivity, 0, alarmIntent, PendingIntentFlags.NoCreate | PendingIntentFlags.Immutable) :
                PendingIntent.GetBroadcast(mainActivity, 0, alarmIntent, PendingIntentFlags.NoCreate);

            if (alreadyUsedPendingIntent != null)
            {
                using (LogContext.PushProperty("BackgroundTimeSyncService", 1))
                {
                    logger.LogDebug("Found already scheduled sync, so cancelling it now ...");
                    alarmManager.Cancel(alreadyUsedPendingIntent);
                }
            }
        }
    }
}
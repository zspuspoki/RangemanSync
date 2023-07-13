using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using Java.Lang;
using Microsoft.Extensions.Logging;
using Rangeman;
using Rangeman.Services.BluetoothConnector;
using Rangeman.Services.NTP;
using Rangeman.Services.WatchDataSender;
using RangemanSync.Android.Services.BackgroundTimeSync;
using Serilog.Context;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using static Android.OS.PowerManager;
using Handler = Android.OS.Handler;

namespace RangemanSync.Android.Services
{
    [Service(Label = "BackgroundTimeSyncService", Name = "com.Szilamer.RangemanSync.BackgroundTimeSyncService")]
    public class BackgroundTimeSyncService : Service
    {
        public const string AlarmIntentName = "AlarmIntent";

        private const string ChannelId = "channel_01";

        private static readonly string TAG = typeof(BackgroundTimeSyncService).FullName;

        /// <summary>
        ///     The notification manager.
        /// </summary>
        private NotificationManager _notificationManager;
        
        private BluetoothConnectorService bluetoothConnectorService;

        private ILoggerFactory loggerFactory;

        private ILogger<BackgroundTimeSyncService> logger;

        private string ntpServer;

        private double compensationSeconds;

        private AlarmBroadcastReceiver alarmBroadcastReceiver;

        private IBinder binder;

        Handler handler;
        Action runnable;

        public BackgroundTimeSyncService()
        {
            var appShell = ((AppShell)App.Current.MainPage);

            this.loggerFactory =
                (ILoggerFactory)appShell.ServiceProvider.GetService(typeof(ILoggerFactory));

            this.logger = loggerFactory.CreateLogger<BackgroundTimeSyncService>();

            bluetoothConnectorService =
                (BluetoothConnectorService)appShell.ServiceProvider.GetService(typeof(BluetoothConnectorService));

            alarmBroadcastReceiver = new AlarmBroadcastReceiver(this, logger);
            
            Application.Context.RegisterReceiver(this.alarmBroadcastReceiver, new IntentFilter(AlarmIntentName));
        }

        public override void OnCreate()
        {
            base.OnCreate();

            var handlerThread = new HandlerThread(TAG);
            handlerThread.Start();

            handler = new Handler(handlerThread.Looper);

            _notificationManager = (NotificationManager)GetSystemService(NotificationService);

            // Android O requires a Notification Channel.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var name = GetString(Resource.String.app_name);
                // Create the channel for the notification
                var mChannel = new NotificationChannel(ChannelId, name, NotificationImportance.Low);

                // Set the Notification Channel for the Notification Manager.
                _notificationManager.CreateNotificationChannel(mChannel);
            }

            // This Action is only for demonstration purposes.
            runnable = new Action(async () =>
            {
                if (bluetoothConnectorService == null)
                {
                    using (LogContext.PushProperty("BackgroundTimeSyncService", 1))
                    {
                        logger.LogDebug("Error ! The bluetooth connector service is unavailable. ");
                    }
                }
                else
                {
                    await SendTime();

                    ScheduleSystemAlarm(cancelAlreadyScheduledAlarm: false);

                    this.alarmBroadcastReceiver.ReleaseWakeLock();
                }
            });
        }

        public override void OnDestroy()
        {
            using (LogContext.PushProperty("BackgroundTimeSyncService", 1))
            {
                logger.LogDebug("OnDestroy: The started service is shutting down.");
            }

            // Stop the handler.
            handler.RemoveCallbacks(runnable);

            // Remove the notification from the status bar.
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Cancel(Constants.SERVICE_RUNNING_NOTIFICATION_ID);

            bluetoothConnectorService = null;

            CancelAlreadyScheduledAlarm();

            base.OnDestroy();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            using (LogContext.PushProperty("BackgroundTimeSyncService", 1))
            {
                if (intent.Action.Equals(Constants.ACTION_START_SERVICE))
                {
                    logger.LogDebug("OnStartCommand: The background time sync service is starting");

                    ntpServer = intent.GetStringExtra(Constants.START_SERVICE_NTP_SERVER);
                    compensationSeconds = intent.Extras.GetDouble(Constants.START_SERVICE_COMPENSATION_SECONDS);

                    RegisterForegroundService();

                    ScheduleSystemAlarm(cancelAlreadyScheduledAlarm: true);
                }
            }

            // This tells Android not to restart the service if it is killed to reclaim resources.
            return StartCommandResult.Sticky;
        }

        private void ScheduleSystemAlarm(bool cancelAlreadyScheduledAlarm)
        {
            var alarmIntent = new Intent(AlarmIntentName);
            alarmIntent.PutExtra(Constants.START_SERVICE_COMPENSATION_SECONDS, compensationSeconds);
            alarmIntent.PutExtra(Constants.START_SERVICE_NTP_SERVER, ntpServer);

            var alarmManager = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService);

            if (cancelAlreadyScheduledAlarm)
            {
                CancelAlreadyScheduledAlarm(alarmIntent, alarmManager);
            }

            var pending = (Build.VERSION.SdkInt >= BuildVersionCodes.M) ?
                PendingIntent.GetBroadcast(this, 0, alarmIntent, PendingIntentFlags.Immutable) :
                PendingIntent.GetBroadcast(this, 0, alarmIntent, PendingIntentFlags.UpdateCurrent);

            using (LogContext.PushProperty("BackgroundTimeSyncService", 1))
            {
                try
                {
                    logger.LogDebug("Scheduling next time sync ... ( the device will be woken up if necessary )");

                    var timeSyncScheduler = new TimeSyncScheduler();
                    if (alarmManager.CanScheduleExactAlarms())
                    {
                        logger.LogDebug("Can schedule exact alarms");
                    }

                    alarmManager.SetExactAndAllowWhileIdle(AlarmType.ElapsedRealtimeWakeup, timeSyncScheduler.GetTriggerMilis(), pending);
                }
                catch
                {
                    logger.LogDebug("An unexpected error occured during scheduling next sync");
                }
            }
        }

        private void CancelAlreadyScheduledAlarm()
        {
            var alarmManager = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService);
            CancelAlreadyScheduledAlarm(new Intent(AlarmIntentName), alarmManager);
        }

        private void CancelAlreadyScheduledAlarm(Intent alarmIntent, AlarmManager alarmManager)
        {
            var alreadyUsedPendingIntent = (Build.VERSION.SdkInt >= BuildVersionCodes.M) ?
                PendingIntent.GetBroadcast(this, 0, alarmIntent, PendingIntentFlags.NoCreate | PendingIntentFlags.Immutable) :
                PendingIntent.GetBroadcast(this, 0, alarmIntent, PendingIntentFlags.NoCreate);

            if (alreadyUsedPendingIntent != null)
            {
                using (LogContext.PushProperty("BackgroundTimeSyncService", 1))
                {
                    logger.LogDebug("Found already scheduled sync, so cancelling it now ...");
                    alarmManager.Cancel(alreadyUsedPendingIntent);
                }
            }
        }

        private async Task SendTime()
        {
            using (LogContext.PushProperty("BackgroundTimeSyncService", 1))
            {
                await bluetoothConnectorService.FindAndConnectToWatch((message) => _ = message,
                    async (connection) =>
                    {
                        logger.LogDebug("BackgroundTimeService - Device Connection was successful");

                        var watchDataSettingSenderService = new WatchDataSettingSenderService(connection, loggerFactory);


                        DateTime? currentTime = null;

                        currentTime = await GetNtpServerTime(currentTime);

                        if (currentTime != null)
                        {
                            logger.LogDebug("SendTimeToTheWatch() - currentTime is not null");

                            logger.LogDebug($"Compensation seconds = {compensationSeconds}");

                            currentTime = currentTime.Value.AddSeconds(compensationSeconds);

                            await SendCommandsToTheWatch(watchDataSettingSenderService, currentTime);
                        }

                        logger.LogDebug("BackgroundTimeService - after awaiting SendTime()");

                        return true;
                    },
                    watchCommandExecutionFailed: async () =>
                    {
                        logger.LogDebug("BackgroundTimeService - Watch command execution wasn't successfull this time, so setting back timeSyncIsRunning flag to false");
                        return true;
                    },
                    beforeStartScanningMethod: null);
            }
        }

        private async Task<DateTime?> GetNtpServerTime(DateTime? currentTime)
        {
            try
            {
                logger.LogDebug($"Getting NTP server time from {ntpServer}");

                currentTime = await NtpClient.GetNetworkTimeAsync(ntpServer);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "An unexpected error occured during getting the time from the NTP server");
            }

            return currentTime;
        }

        private async Task SendCommandsToTheWatch(WatchDataSettingSenderService watchDataSettingSenderService, DateTime? currentTime)
        {
            try
            {
                Location location = null;

                try
                {
                    location = await Geolocation.GetLastKnownLocationAsync();
                }
                catch (System.Exception ex)
                {
                    logger.LogDebug("Current location is unknown");
                }

                await watchDataSettingSenderService.SendTime((ushort)currentTime.Value.Year, (byte)currentTime.Value.Month, (byte)currentTime.Value.Day,
                    (byte)currentTime.Value.Hour, (byte)currentTime.Value.Minute, (byte)currentTime.Value.Second,
                    (byte)currentTime.Value.DayOfWeek, 0, location?.Latitude, location?.Longitude);
            }
            catch (System.Exception ex)
            {
                logger.LogDebug("The watch forced closing SendTime");
            }
        }

        private void RegisterForegroundService()
        {
            var text = "test_text";
            var title = "test_title";

            var builder = new NotificationCompat.Builder(this, ChannelId)
              .SetContentText(text)
              .SetContentTitle(title)
              .SetOngoing(true)
              .SetPriority((int)NotificationPriority.Low)
              .SetTicker(text)
              .SetWhen(JavaSystem.CurrentTimeMillis());

            // Set the Channel ID for Android O.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O) builder.SetChannelId(ChannelId);

            // Enlist this instance of the service as a foreground service
            StartForeground(Constants.SERVICE_RUNNING_NOTIFICATION_ID, builder.Build());
        }

        public override IBinder OnBind(Intent intent)
        {
            binder = new BackgroundTimeSyncServiceBinder(this);
            return binder;
        }

        public void StartTimeSyncAtScheduledTime()
        {
            handler.PostDelayed(runnable, Constants.SHORT_DELAY_BETWEEN_CHECKSYNCTIME);
        }

        [IntentFilter(new[] { AlarmIntentName })]
        public class AlarmBroadcastReceiver : BroadcastReceiver
        {
            private static WakeLock wakeLock;
            private readonly BackgroundTimeSyncService backgroundTimeSyncService;
            private readonly ILogger<BackgroundTimeSyncService> logger;

            public AlarmBroadcastReceiver(BackgroundTimeSyncService backgroundTimeSyncService, ILogger<BackgroundTimeSyncService> logger)
            {
                this.backgroundTimeSyncService = backgroundTimeSyncService;
                this.logger = logger;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                using (LogContext.PushProperty("BackgroundTimeSyncService", 1))
                {
                    try
                    {
                        PowerManager powerManager = Application.Context.GetSystemService(Context.PowerService) as PowerManager;
                        wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "ServiceWakeLock");
                        wakeLock.SetReferenceCounted(false);

                        logger.LogDebug("Received signal in broadcast receiver. Starting time sync routine ...");

                        this.backgroundTimeSyncService.StartTimeSyncAtScheduledTime();

                    }
                    catch (System.Exception ex)
                    {
                        logger.LogDebug("An unexpected error occured during starting sync routine in the broadcast receiver");
                    }
                }
            }

            public void ReleaseWakeLock()
            {
                if (wakeLock != null)
                {
                    wakeLock.Release();
                    wakeLock = null;
                }
            }
        }
    }
}
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Java.Lang;
using Microsoft.Extensions.Logging;
using Rangeman;
using Rangeman.Services.BackgroundTimeSyncService;
using Rangeman.Services.BluetoothConnector;
using Rangeman.Services.NTP;
using Rangeman.Services.WatchDataSender;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Android.OS.PowerManager;
using Handler = Android.OS.Handler;

namespace RangemanSync.Android.Services
{
    [Service]
    public class BackgroundTimeSyncService : Service
    {
        private const string ChannelId = "channel_01";
        private static readonly string TAG = typeof(BackgroundTimeSyncService).FullName;

        /// <summary>
        ///     The notification manager.
        /// </summary>
        private NotificationManager _notificationManager;
        
        private BluetoothConnectorService bluetoothConnectorService;

        private ILoggerFactory loggerFactory;

        private ILogger<BackgroundTimeSyncService> logger;

        private bool timeSyncIsRunning;

        private WakeLock wakeLock;

        private string ntpServer;

        private double compensationSeconds;

        private ITimeSyncServiceStatus timeSyncServiceStatus;

        Handler handler;
        Action runnable;

        public BackgroundTimeSyncService()
        {
            PowerManager powerManager = Application.Context.GetSystemService(Context.PowerService) as PowerManager;
            wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "ServiceWakeLock");
            wakeLock.SetReferenceCounted(false);
        }

        public override void OnCreate()
        {
            base.OnCreate();

            if (wakeLock != null)
                wakeLock.Acquire();

            var appShell = ((AppShell)App.Current.MainPage);

            this.loggerFactory =
                (ILoggerFactory)appShell.ServiceProvider.GetService(typeof(ILoggerFactory));

            this.logger = loggerFactory.CreateLogger<BackgroundTimeSyncService>();

            bluetoothConnectorService =
                (BluetoothConnectorService)appShell.ServiceProvider.GetService(typeof(BluetoothConnectorService));

            timeSyncServiceStatus =
                (ITimeSyncServiceStatus)appShell.ServiceProvider.GetService(typeof(ITimeSyncServiceStatus));

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
            runnable = new Action(() =>
            {
                if (bluetoothConnectorService == null)
                {
                    //TODO: Log
                }
                else
                {

                    var syncTimes = new List<DateTime>
                    {
                        new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 30, 0),
                        new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 30, 0),
                        new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 18,30 , 0),
                        new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 30, 0)
                    };

                    bool hasAtLeastOneSyncTimeCLoserThanOneHour = false;

                    foreach(var syncTime in syncTimes)
                    {
                        var timeDiff = syncTime - DateTime.Now;
                        if (timeDiff.TotalMinutes > 0 && timeDiff.TotalMinutes <= 60)
                        {
                            hasAtLeastOneSyncTimeCLoserThanOneHour = true;
                        }

                        if( timeDiff.TotalSeconds > 0 && timeDiff.TotalSeconds <= 60)
                        {
                            if (!timeSyncIsRunning)
                            {
                                timeSyncIsRunning = true;
                                SendTime();
                            }
                        }
                    }

                    if (hasAtLeastOneSyncTimeCLoserThanOneHour)
                    {
                        handler.PostDelayed(runnable, Constants.SHORT_DELAY_BETWEEN_CHECKSYNCTIME);
                    }
                    else
                    {
                        handler.PostDelayed(runnable, Constants.LONG_DELAY_BETWEEN_CHECKSYNCTIME);
                    }
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

            if (wakeLock != null)
            {
                wakeLock.Release();
                wakeLock = null;
            }

            bluetoothConnectorService = null;
            timeSyncServiceStatus.IsStarted = false;

            base.OnDestroy();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            using (LogContext.PushProperty("BackgroundTimeSyncService", 1))
            {
                if (intent.Action.Equals(Constants.ACTION_START_SERVICE))
                {
                    if (timeSyncServiceStatus.IsStarted)
                    {
                        logger.LogDebug("The background time sync service is alrady running.");
                    }
                    else
                    {
                        logger.LogDebug("OnStartCommand: The background time sync service is starting");

                        ntpServer = intent.GetStringExtra(Constants.START_SERVICE_NTP_SERVER);
                        compensationSeconds = intent.Extras.GetDouble(Constants.START_SERVICE_COMPENSATION_SECONDS);

                        RegisterForegroundService();
                        handler.PostDelayed(runnable, Constants.SHORT_DELAY_BETWEEN_CHECKSYNCTIME);
                        timeSyncServiceStatus.IsStarted = true;
                    }
                }
                else if (intent.Action.Equals(Constants.ACTION_STOP_SERVICE))
                {
                    logger.LogDebug("OnStartCommand: The background time sync service is stopping");
                    bluetoothConnectorService = null;
                    StopForeground(true);
                    StopSelf();
                    timeSyncServiceStatus.IsStarted = false;

                }
            }

            // This tells Android not to restart the service if it is killed to reclaim resources.
            return StartCommandResult.Sticky;
        }
        private async void SendTime()
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

                        timeSyncIsRunning = false;

                        return true;
                    },
                    watchCommandExecutionFailed: async () =>
                    {
                        logger.LogDebug("BackgroundTimeService - Watch command execution wasn't successfull this time, so setting back timeSyncIsRunning flag to false");

                        timeSyncIsRunning = false;
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
                await watchDataSettingSenderService.SendTime((ushort)currentTime.Value.Year, (byte)currentTime.Value.Month, (byte)currentTime.Value.Day,
                    (byte)currentTime.Value.Hour, (byte)currentTime.Value.Minute, (byte)currentTime.Value.Second,
                    (byte)currentTime.Value.DayOfWeek, 0);
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
            // Return null because this is a pure started service. A hybrid service would return a binder that would
            // allow access to the GetFormattedStamp() method.
            return null;
        }
    }
}
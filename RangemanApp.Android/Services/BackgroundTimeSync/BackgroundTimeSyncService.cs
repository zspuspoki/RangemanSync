using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Java.Lang;
using Java.Util.Logging;
using Microsoft.Extensions.Logging;
using nexus.protocols.ble;
using Rangeman;
using Rangeman.Services.BluetoothConnector;
using Rangeman.Services.NTP;
using Rangeman.Services.WatchDataSender;
using Rangeman.Views.Time;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        bool isStarted;
        Handler handler;
        Action runnable;

        public override void OnCreate()
        {
            base.OnCreate();

            var appShell = ((AppShell)App.Current.MainPage);

            this.loggerFactory =
                (ILoggerFactory)appShell.ServiceProvider.GetService(typeof(ILoggerFactory));

            this.logger = loggerFactory.CreateLogger<BackgroundTimeSyncService>();

            bluetoothConnectorService =
                (BluetoothConnectorService)appShell.ServiceProvider.GetService(typeof(BluetoothConnectorService));

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
                        new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 18, 30, 0),
                        new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 30, 0)
                    };

                    foreach(var syncTime in syncTimes)
                    {
                        var timeDiff = syncTime - DateTime.Now;
                        if( timeDiff.TotalSeconds > 0 && timeDiff.TotalSeconds <= 60)
                        {
                            if (!timeSyncIsRunning)
                            {
                                timeSyncIsRunning = true;
                                SendTime();
                            }
                        }
                    }

                    handler.PostDelayed(runnable, Constants.DELAY_BETWEEN_LOG_MESSAGES);
                }
            });
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (intent.Action.Equals(Constants.ACTION_START_SERVICE))
            {
                if (isStarted)
                {
                    //Log.Info(TAG, "OnStartCommand: The service is already running.");
                }
                else
                {
                    //Log.Info(TAG, "OnStartCommand: The service is starting.");
                    RegisterForegroundService();
                    handler.PostDelayed(runnable, Constants.DELAY_BETWEEN_LOG_MESSAGES);
                    isStarted = true;
                }
            }
            else if (intent.Action.Equals(Constants.ACTION_STOP_SERVICE))
            {
                //Log.Info(TAG, "OnStartCommand: The service is stopping.");
                bluetoothConnectorService = null;
                StopForeground(true);
                StopSelf();
                isStarted = false;

            }

            // This tells Android not to restart the service if it is killed to reclaim resources.
            return StartCommandResult.Sticky;
        }
        private async void SendTime()
        {
            await bluetoothConnectorService.FindAndConnectToWatch((message) =>_ = message,
                async (connection) =>
                {
                    logger.LogDebug("BackgroundTimeService - Device Connection was successful");

                    var watchDataSettingSenderService = new WatchDataSettingSenderService(connection, loggerFactory);


                    DateTime? currentTime = null;

                    currentTime = await GetNtpServerTime(currentTime);

                    if (currentTime != null)
                    {
                        logger.LogDebug("SendTimeToTheWatch() - currentTime is not null");

                        currentTime = currentTime.Value.AddSeconds(5000);

                        await SendCommandsToTheWatch(watchDataSettingSenderService, currentTime);
                    }

                    logger.LogDebug("BackgroundTimeService - after awaiting SendTime()");

                    timeSyncIsRunning = false;

                    return true;
                },
                async () =>
                {
                    return true;
                },
                null);
        }

        private async Task<DateTime?> GetNtpServerTime(DateTime? currentTime)
        {
            try
            {
                currentTime = await NtpClient.GetNetworkTimeAsync("time.google.com");
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
                logger.LogError(ex, "NTP - An unexpected error occured during sending the watch commands to the watch");
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
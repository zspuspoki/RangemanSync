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
using System;
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

        bool isStarted;
        Handler handler;
        Action runnable;

        public override void OnCreate()
        {
            base.OnCreate();

            var appShell = ((AppShell)App.Current.MainPage);
            var bluetoothLowEnergyAdapter =
                (IBluetoothLowEnergyAdapter) appShell.ServiceProvider.GetService(typeof(IBluetoothLowEnergyAdapter));

            var loggerBluetoothConnectorService =
                (ILogger<BluetoothConnectorService>) appShell.ServiceProvider.GetService(typeof(ILogger<BluetoothConnectorService>));

            bluetoothConnectorService = new BluetoothConnectorService(bluetoothLowEnergyAdapter, loggerBluetoothConnectorService);

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
        private void SendTime()
        {

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
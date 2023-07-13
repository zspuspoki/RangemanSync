﻿using Android.App;
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
        private Intent stopServiceIntent;
        private ILoggerFactory loggerFactory;
        private ILogger<TimeSyncServiceStarter> logger;

        private string ntpServer;
        private double? compensationSeconds;

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

            stopServiceIntent = new Intent(mainActivity, typeof(BackgroundTimeSyncService));
            stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);
        }

        public void StartWithLastUsedParameters()
        {
            using (LogContext.PushProperty("BackgroundTimeSyncService", 1))
            {
                logger.LogDebug("Starting alarm on destroying app's main activity.");

                if(ntpServer != null && compensationSeconds.HasValue)
                {
                    Start(ntpServer, compensationSeconds.Value, true);
                }
            }
        }

        public void Start(string ntpServer, double compensationSeconds, bool cancelPrevious)
        {
            using (LogContext.PushProperty("BackgroundTimeSyncService", 1))
            {
                var startServiceIntent = new Intent(mainActivity, typeof(BackgroundTimeSyncService));
                startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);

                startServiceIntent.PutExtra(Constants.START_SERVICE_COMPENSATION_SECONDS, compensationSeconds);
                startServiceIntent.PutExtra(Constants.START_SERVICE_NTP_SERVER, ntpServer);

                logger.LogDebug("Before starting foreground service");

                mainActivity.StartForegroundService(startServiceIntent);

                logger.LogDebug("After starting foreground service");
            }
        }

        public void Stop()
        {
            try
            {
                using (LogContext.PushProperty("BackgroundTimeSyncService", 1))
                {
                    logger.LogDebug("Stopping service and the alarm associated with it ...");

                    mainActivity.StopService(stopServiceIntent);
                }
            }
            catch(System.Exception ex)
            {
                logger.LogError(ex, "Error occured during stopping service and AlarmManager");
            }
        }

    }
}
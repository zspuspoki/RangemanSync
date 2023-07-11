using Android.OS;
using Microsoft.Extensions.Logging;
using Rangeman;
using Serilog.Context;
using System;
using System.Collections.Generic;

namespace RangemanSync.Android.Services.BackgroundTimeSync
{
    public class TimeSyncScheduler
    {
        private ILoggerFactory loggerFactory;
        private ILogger<TimeSyncScheduler> logger;

        public TimeSyncScheduler()
        {
            var appShell = ((AppShell)App.Current.MainPage);

            this.loggerFactory =
              (ILoggerFactory)appShell.ServiceProvider.GetService(typeof(ILoggerFactory));

            this.logger = loggerFactory.CreateLogger<TimeSyncScheduler>();

        }

        public long GetTriggerMilis()
        {
            var syncTimes = new List<DateTime>
                    {
                        new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 30, 0),
                        new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 30, 0),
                        new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 18,30 , 0),
                        new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 30, 0)
                    };

            foreach (var syncTime in syncTimes)
            {
                if(DateTime.Now < syncTime)
                {
                    using (LogContext.PushProperty("BackgroundTimeSyncService", 1))
                    {
                        logger.LogDebug($"Next time sync will be at {syncTime}");
                    }

                    TimeSpan span = syncTime - DateTime.Now;
                    return (long)(SystemClock.ElapsedRealtime() + span.TotalMilliseconds);
                }
            }

            return -1;
        }
    }
}
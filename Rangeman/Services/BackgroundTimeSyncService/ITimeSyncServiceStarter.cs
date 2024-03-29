﻿namespace Rangeman.Services.BackgroundTimeSyncService
{
    public interface ITimeSyncServiceStarter
    {
        void Start(string ntpServer, double compensationSeconds, bool cancelPrevious = true);

        void Stop();
    }
}
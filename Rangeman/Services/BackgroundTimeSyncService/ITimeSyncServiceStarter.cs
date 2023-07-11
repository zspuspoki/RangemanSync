namespace Rangeman.Services.BackgroundTimeSyncService
{
    public interface ITimeSyncServiceStarter
    {
        void Start(string ntpServer, double compensationSeconds);
    }
}
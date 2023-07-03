namespace RangemanSync.Android
{
    public static class Constants
    {
        public const int DELAY_BETWEEN_LOG_MESSAGES = 5000;
        public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;
        public const string SERVICE_STARTED_KEY = "has_service_been_started";
        public const string ACTION_START_SERVICE = "RangemanApp.Android.action.START_SERVICE";
        public const string ACTION_STOP_SERVICE = "RangemanApp.Android.action.STOP_SERVICE";
        public const string START_SERVICE_COMPENSATION_SECONDS = "StartServiceCompensationValue";
        public const string START_SERVICE_NTP_SERVER = "NTPServer";
    }
}
namespace Rangeman
{
    public static class Constants
    {
        public static string LogSubFolder = "RangemanSyncLogs";
        public static string PrefKeyGPX = "SaveGPX";
        public static string PrefSaveCoordinatesData = "SaveCoordinatesData";
        public static string MbTilesFile = "MbTilesFile";

        #region NTP Time tab
        public static string NTPServer = nameof(NTPServer);
        public static string SecondsCompensation = nameof(SecondsCompensation);
        #endregion

        #region Background time sync
        public static string ServiceState = nameof(ServiceState);
        #endregion
    }
}
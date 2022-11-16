namespace Rangeman
{
    public class AppShellViewModel : ViewModelBase
    {
        private bool downloadPageIsEnabled;
        private bool mapPageIsEnabled;
        private bool configPageIsEnabled;

        public bool DownloadPageIsEnabled { get => downloadPageIsEnabled; set { downloadPageIsEnabled = value; OnPropertyChanged("DownloadPageIsEnabled"); } }
        public bool MapPageIsEnabled { get => mapPageIsEnabled; set { mapPageIsEnabled = value; OnPropertyChanged("MapPageIsEnabled"); } }
        public bool ConfigPageIsEnabled { get => configPageIsEnabled; set { configPageIsEnabled = value; OnPropertyChanged("ConfigPageIsEnabled"); } }
    }
}
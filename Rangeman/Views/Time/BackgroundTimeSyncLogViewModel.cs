using System;
using System.IO;
using System.Text;

namespace Rangeman.Views.Time
{
    public class BackgroundTimeSyncLogViewModel : ViewModelBase
    {
        private string logMessages;
        public string LogMessages { get => logMessages; set { logMessages = value; OnPropertyChanged(nameof(LogMessages)); } }

        public BackgroundTimeSyncLogViewModel()
        {
            ReloadLogMessages();
        }

        public void ReloadLogMessages()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var logDir = Path.Combine(path, Constants.LogSubFolder);
            var filesNames = Directory.GetFiles(logDir);

            LogMessages = "";
            StringBuilder sb = new StringBuilder();

            foreach (var fileName in filesNames)
            {
                if(fileName.Contains("TimeSyncService"))
                {
                    sb.Append(File.ReadAllText(fileName));
                }
            }

            LogMessages = sb.ToString();
        }
    }
}
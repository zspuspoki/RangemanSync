using System;
using System.IO;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Rangeman
{
    internal class ConfigPageViewModel : ViewModelBase
    {
        private bool useMbTilesChecked;
        private bool sendLogFilesChecked;

        private ICommand applyCommand;
        private readonly MapPageViewModel mapPageViewModel;

        public ConfigPageViewModel(MapPageViewModel mapPageViewModel)
        {
            this.mapPageViewModel = mapPageViewModel;
        }

        public bool UseMbTilesChecked 
        { 
            get => useMbTilesChecked; 
            set 
            { 
                useMbTilesChecked = value; 
                OnPropertyChanged("UseMbTilesChecked"); 
            } 
        }

        public bool SendLogFilesChecked
        {
            get => sendLogFilesChecked;
            set
            {
                sendLogFilesChecked = value;
                OnPropertyChanged("SendLogFilesChecked");
            }
        }

        public ICommand ApplyCommand
        {
            get
            {
                if(applyCommand == null)
                {
                    applyCommand = new Command((o) => ApplySettings(), (o) => CanApplySettings());
                }

                return applyCommand;
            }
        }

        private bool CanApplySettings()
        {
            return true;
        }

        private async void ApplySettings()
        {
            if (UseMbTilesChecked)
            {
                mapPageViewModel.UpdateMapToUseMbTilesFile();
            }

            if(sendLogFilesChecked)
            {
                SendEmailToDevSupport();
            }
        }

        private async void SendEmailToDevSupport()
        {
            var message = new EmailMessage
            {
                Subject = "Error report",
                Body = "Dear Support, Something is wrong with my app, please help. I've attached the logs.",
            };

            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var logDir = Path.Combine(path, Constants.LogSubFolder);
            var files = Directory.GetFiles(logDir);

            foreach(var file in files)
            {
                message.Attachments.Add(new EmailAttachment(file));
            }

            await Email.ComposeAsync(message);
            SendLogFilesChecked = false;
        }
    }
}
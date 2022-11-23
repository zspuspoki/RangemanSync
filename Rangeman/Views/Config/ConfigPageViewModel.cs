using System;
using System.IO;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Rangeman
{
    internal class ConfigPageViewModel : ViewModelBase
    {
        private string progressMessage = "";
        private bool useMbTilesChecked;
        private bool sendLogFilesChecked;
        private bool showCalculatedDistanceFromYourPosition;

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
                SetPressApplyButtonProgressMessage();
            }
        }

        public bool ShowCalculatedDistanceFromYourPosition
        { 
            get => showCalculatedDistanceFromYourPosition; 
            set 
            {
                showCalculatedDistanceFromYourPosition = value; 
                OnPropertyChanged("ShowCalculatedDistanceFromYourPosition");
                SetPressApplyButtonProgressMessage();
            } 
        }

        public bool SendLogFilesChecked
        {
            get => sendLogFilesChecked;
            set
            {
                sendLogFilesChecked = value;
                OnPropertyChanged("SendLogFilesChecked");
                SetPressApplyButtonProgressMessage();
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

        public string ProgressMessage { get => progressMessage; set { progressMessage = value; OnPropertyChanged("ProgressMessage"); } }

        private void SetPressApplyButtonProgressMessage()
        {
            ProgressMessage = "Changes haven't been applied yet. Please press the apply button to do this now.";
        }

        private void SetApplyFinishedProgressMessage()
        {
            ProgressMessage = $"All changes have been applied. Time: {DateTime.Now}";
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
            else
            {
                mapPageViewModel.UpdateMapToUseWebBasedMbTiles();
            }

            if(sendLogFilesChecked)
            {
                SendEmailToDevSupport();
            }

            mapPageViewModel.ShowCalculatedDistances = ShowCalculatedDistanceFromYourPosition;
            SetApplyFinishedProgressMessage();
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
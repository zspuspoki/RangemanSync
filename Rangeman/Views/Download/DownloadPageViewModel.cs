using Android.Content;
using employeeID;
using Rangeman.DataExtractors.Data;
using Rangeman.Services.BluetoothConnector;
using Rangeman.Views.Download;
using SharpGPX;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Xamarin.Forms;

namespace Rangeman
{
    public class DownloadPageViewModel : ViewModelBase
    {
        private string progressMessage;
        private bool watchCommandButtonsAreVisible = true;
        private bool disconnectButtonIsVisible = false;
        private readonly BluetoothConnectorService bluetoothConnectorService;
        private readonly AppShellViewModel appShellViewModel;
        private readonly IDownloadPageView downloadPageView;
        private bool disconnectButtonCanBePressed = true;
        private bool downloadHeadersButtonCanBePressed = true;
        private bool saveGPXButtonCanBePressed = true;

        private ICommand disconnectCommand;
        private ICommand downloadHeadersCommand;
        private ICommand saveGPXCommand;

        public DownloadPageViewModel(Context context, BluetoothConnectorService bluetoothConnectorService, AppShellViewModel appShellViewModel, IDownloadPageView downloadPageView)
        {
            Context = context;
            this.bluetoothConnectorService = bluetoothConnectorService;
            this.appShellViewModel = appShellViewModel;
            this.downloadPageView = downloadPageView;
        }

        #region Button handlers
        private async void DisconnectButton_Clicked()
        {
            disconnectButtonCanBePressed = false;
            await bluetoothConnectorService.DisconnectFromWatch(SetProgressMessage);
            DisconnectButtonIsVisible = false;
            disconnectButtonCanBePressed = true;
        }

        private async void DownloadHeaders_Clicked()
        {
            Debug.WriteLine("--- MainPage - start DownloadHeaders_Clicked");
            SetProgressMessage("Looking for Casio GPR-B1000 device. Please connect your watch.");

            downloadHeadersButtonCanBePressed = false;
            DisableOtherTabs();

            await bluetoothConnectorService.FindAndConnectToWatch(SetProgressMessage, async (connection) =>
            {
                var logPointMemoryService = new LogPointMemoryExtractorService(connection);
                logPointMemoryService.ProgressChanged += LogPointMemoryService_ProgressChanged;
                var headersTask = logPointMemoryService.GetHeaderDataAsync();
                var headers = await headersTask;
                LogHeaderList.Clear();
                headers.ForEach(h => LogHeaderList.Add(h.ToViewModel()));

                logPointMemoryService.ProgressChanged -= LogPointMemoryService_ProgressChanged;

                DisconnectButtonIsVisible = false;

                return true;
            },
            async () =>
            {
                SetProgressMessage("An error occured during sending watch commands. Please try to connect again");
                return true;
            },
            () => DisconnectButtonIsVisible = true);

            EnableOtherTabs();
            downloadHeadersButtonCanBePressed = true;
        }

        private async void DownloadSaveGPXButton_Clicked()
        {
            Debug.WriteLine("--- MainPage - start DownloadSaveGPXButton_Clicked");
            SetProgressMessage("Looking for Casio GPR-B1000 device. Please connect your watch.");

            saveGPXButtonCanBePressed = false;
            DisableOtherTabs();

            await bluetoothConnectorService.FindAndConnectToWatch(SetProgressMessage, async (connection) =>
            {
                if (SelectedLogHeader != null)
                {
                    Debug.WriteLine("DownloadSaveGPXButton_Clicked : Before GetLogDataAsync");
                    Debug.WriteLine($"Selected ordinal number: {SelectedLogHeader.OrdinalNumber}");
                    var logPointMemoryService = new LogPointMemoryExtractorService(connection);
                    logPointMemoryService.ProgressChanged += LogPointMemoryService_ProgressChanged;
                    var selectedHeader = SelectedLogHeader;
                    var logDataEntries = await logPointMemoryService.GetLogDataAsync(selectedHeader.OrdinalNumber,
                        selectedHeader.DataSize,
                        selectedHeader.DataCount,
                        selectedHeader.LogAddress,
                        selectedHeader.LogTotalLength);

                    logPointMemoryService.ProgressChanged -= LogPointMemoryService_ProgressChanged;

                    SaveGPXFile(logDataEntries);

                    DisconnectButtonIsVisible = false;

                    return true;
                }
                else
                {
                    Debug.WriteLine("DownloadSaveGPXButton_Clicked : One log header entry should be selected");
                    return false;
                }
            },
            async () =>
            {
                SetProgressMessage("An error occured during sending watch commands. Please try to connect again");
                return true;
            },
            () => DisconnectButtonIsVisible = true);

            EnableOtherTabs();
            saveGPXButtonCanBePressed = true;
            //Save selected log header as GPX
        }

        private void DisableOtherTabs()
        {
            appShellViewModel.ConfigPageIsEnabled = false;
            appShellViewModel.MapPageIsEnabled = false;
        }

        private void EnableOtherTabs()
        {
            appShellViewModel.ConfigPageIsEnabled = true;
            appShellViewModel.MapPageIsEnabled = true;
        }

        private void LogPointMemoryService_ProgressChanged(object sender, WatchDataReceiver.DataReceiverProgressEventArgs e)
        {
            SetProgressMessage(e.Text);
        }

        private async void SaveGPXFile(List<LogData> logDataEntries)
        {
            GpxClass gpx = new GpxClass();

            gpx.Metadata.time = SelectedLogHeader.HeaderTime;
            gpx.Metadata.timeSpecified = true;
            gpx.Metadata.desc = "Track exported from Casio GPR-B1000 watch";

            gpx.Tracks.Add(new SharpGPX.GPX1_1.trkType());
            gpx.Tracks[0].trkseg.Add(new SharpGPX.GPX1_1.trksegType());

            foreach (var logEntry in logDataEntries)
            {
                var wpt = new SharpGPX.GPX1_1.wptType
                {
                    lat = (decimal)logEntry.Latitude,
                    lon = (decimal)logEntry.Longitude,   // ele tag : pressure -> elevation conversion ?
                    time = logEntry.Date,
                    timeSpecified = true,
                };

                gpx.Tracks[0].trkseg[0].trkpt.Add(wpt);
            }

            var headerTime = SelectedLogHeader.HeaderTime;
            var fileName = $"GPR-B1000-Route-{headerTime.Year}-{headerTime.Month}-{headerTime.Day}-2.gpx";
            var path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            string filePath = Path.Combine(path, fileName);

            if (File.Exists(filePath))
            {
                var action = await downloadPageView.DisplayAlert("Overwrite?", "File already exists. Would you like to overwrite it ?", "Yes", "No");
                if (action)
                {
                    File.Delete(filePath);
                }
                else
                {
                    return;
                }
            }

            gpx.ToFile(filePath);
            await downloadPageView.DisplayAlert("Alert", $"File saved here: {filePath}", "OK");
        }

        private void SetProgressMessage(string message)
        {
            ProgressMessage = message;
        }

        #endregion

        #region Properties
        public Context Context { get; }
        public ObservableCollection<LogHeaderViewModel> LogHeaderList { get; } = new ObservableCollection<LogHeaderViewModel>();
        public LogHeaderViewModel SelectedLogHeader { get; set; }
        public string ProgressMessage { get => progressMessage; set { progressMessage = value; OnPropertyChanged("ProgressMessage"); } }
        
        public bool WatchCommandButtonsAreVisible { get => watchCommandButtonsAreVisible; set { watchCommandButtonsAreVisible = value; OnPropertyChanged("WatchCommandButtonsAreVisible"); } }
        public bool DisconnectButtonIsVisible 
        { 
            get => disconnectButtonIsVisible; 
            set 
            { 
                disconnectButtonIsVisible = value; 
                OnPropertyChanged("DisconnectButtonIsVisible");
                WatchCommandButtonsAreVisible = !value;
            } 
        }

        #region Button commands
        public ICommand DisconnectCommand
        {
            get
            {
                if (disconnectCommand == null)
                {
                    disconnectCommand = new Command((o) => DisconnectButton_Clicked(), (o) => disconnectButtonCanBePressed);
                }

                return disconnectCommand;
            }
        }

        public ICommand DownloadHeadersCommand
        {
            get
            {
                if (downloadHeadersCommand == null)
                {
                    downloadHeadersCommand = new Command((o) => DownloadHeaders_Clicked(), (o) => downloadHeadersButtonCanBePressed);
                }

                return downloadHeadersCommand;
            }
        }

        public ICommand SaveGPXCommand
        {
            get
            {
                if (saveGPXCommand == null)
                {
                    saveGPXCommand = new Command((o) => DownloadSaveGPXButton_Clicked(), (o) => saveGPXButtonCanBePressed);
                }

                return saveGPXCommand;
            }
        }
        #endregion

        #endregion
    }
}
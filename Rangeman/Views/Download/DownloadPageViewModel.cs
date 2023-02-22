using Android.Content;
using Microsoft.Extensions.Logging;
using Rangeman.Services.BluetoothConnector;
using Rangeman.Services.WatchDataReceiver;
using Rangeman.Views.Download;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace Rangeman
{
    public class DownloadPageViewModel : ViewModelBase
    {
        private string progressMessage = "Press the download headers button to start downloading the previously recorded routes from the watch. This message can be closed any time by tapping on it";
        private bool watchCommandButtonsAreVisible = true;
        private bool disconnectButtonIsVisible = false;
        private ILogger<DownloadPageViewModel> logger;
        private readonly BluetoothConnectorService bluetoothConnectorService;
        private readonly AppShellViewModel appShellViewModel;
        private readonly IDownloadPageView downloadPageView;
        private readonly ILoggerFactory loggerFactory;
        private bool disconnectButtonCanBePressed = true;
        private bool downloadHeadersButtonCanBePressed = true;

        private ICommand disconnectCommand;
        private ICommand downloadHeadersCommand;

        public DownloadPageViewModel(Context context, BluetoothConnectorService bluetoothConnectorService, 
            AppShellViewModel appShellViewModel, IDownloadPageView downloadPageView,
            ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<DownloadPageViewModel>();

            logger.LogInformation("Inside Download page VM ctor");

            this.bluetoothConnectorService = bluetoothConnectorService;
            this.appShellViewModel = appShellViewModel;
            this.downloadPageView = downloadPageView;
            this.loggerFactory = loggerFactory;
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
            logger.LogInformation("--- MainPage - start DownloadHeaders_Clicked");

            SetProgressMessage("Looking for Casio GPR-B1000 device. Please connect your watch.");

            downloadHeadersButtonCanBePressed = false;
            DisableOtherTabs();

            await bluetoothConnectorService.FindAndConnectToWatch(SetProgressMessage, async (connection) =>
            {
                var logPointMemoryService = new LogPointMemoryExtractorService(connection, loggerFactory);
                logPointMemoryService.ProgressChanged += LogPointMemoryService_ProgressChanged;
                var headersTask = logPointMemoryService.GetHeaderDataAsync();
                var headers = await headersTask;

                LogHeaderList.Clear();

                if (headers != null && headers.Count > 0)
                {
                    headers.ForEach(h => LogHeaderList.Add(h.ToViewModel()));
                }
                else
                {
                    logger.LogDebug("Headers downloading resulted 0 headers");
                    SetProgressMessage("Headers downloading resulted 0 headers. Please make sure you have recorded routes on the watch. If yes, then please try again because the transmission has been terminated by the watch.");
                }

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

        private void SetProgressMessage(string message)
        {
            ProgressMessage = message;
        }

        #endregion

        #region Properties
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

        #endregion

        #endregion
    }
}
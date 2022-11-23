using Android.Content;
using Rangeman.Views.Map;
using Xamarin.Forms;
using Rangeman.Services.BluetoothConnector;
using Rangeman.WatchDataSender;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Xamarin.Essentials;
using System;

namespace Rangeman
{
    public class MapPageViewModel : ViewModelBase
    {
        private bool showCalculatedDistance = false;
        private bool addressPanelIsVisible = false;
        private bool progressBarIsVisible;
        private string progressBarPercentageMessage;
        private string progressMessage;
        private double progressBarPercentageNumber;

        private NodesViewModel nodesViewModel;
        private AddressPanelViewModel addressPanelViewModel;
        private readonly IMapPageView mapPageView;
        private readonly AppShellViewModel appShellViewModel;
        private readonly BluetoothConnectorService bluetoothConnectorService;
        private readonly ILoggerFactory loggerFactory;
        private RowDefinitionCollection gridViewRows;
        private ILogger<MapPageViewModel> logger;
        private bool watchCommandButtonsAreVisible = true;
        private bool disconnectButtonIsVisible = false;

        private bool sendButtonCanbePressed = true;
        private bool deleteButtonCanbePressed = true;
        private bool selectButtonCanbePressed = true;
        private bool addressButtonCanbePressed = true;
        private bool disconnectButtonCanbePressed = true;

        private ICommand sendCommand;
        private ICommand deleteCommand;
        private ICommand selectCommand;
        private ICommand addressCommand;
        private ICommand disconnectCommand;

        public MapPageViewModel(Context context, NodesViewModel nodesViewModel, 
            AddressPanelViewModel addressPanelViewModel, IMapPageView mapPageView, 
            AppShellViewModel appShellViewModel, BluetoothConnectorService bluetoothConnectorService,
            ILoggerFactory loggerFactory)
        {
            this.nodesViewModel = nodesViewModel;
            this.addressPanelViewModel = addressPanelViewModel;
            this.mapPageView = mapPageView;
            this.appShellViewModel = appShellViewModel;
            this.bluetoothConnectorService = bluetoothConnectorService;
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory.CreateLogger<MapPageViewModel>();

            gridViewRows = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = 0 }
            };
        }

        public async void ShowDistanceFromCurrentPosition(double longitude, double latitude)
        {
            if(!ShowCalculatedDistances)
            {
                return;
            }

            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location != null)
                {
                    var distance = Location.CalculateDistance(location.Latitude, location.Longitude, 
                        latitude, longitude, DistanceUnits.Kilometers);

                    ProgressMessage = $"Distance from the current position: {distance.ToString("N3")} km";
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
                ProgressMessage = "An unexpexcted error occured during calculating the distance. ( current point - selected point)";
            }
        }

        public void UpdateMapToUseMbTilesFile()
        {
            mapPageView.UpdateMapToUseMbTilesFile();            
        }

        public void UpdateMapToUseWebBasedMbTiles()
        {
            mapPageView.UpdateMapToUseWebBasedMbTiles();
        }

        public bool ToggleAddressPanelVisibility()
        {
            if (!addressPanelIsVisible)
            {
                GridViewRows = new RowDefinitionCollection
                {
                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
                };
            }
            else
            {
                GridViewRows = new RowDefinitionCollection
                {
                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                    new RowDefinition { Height = 0 }
                };
            }

            addressPanelIsVisible = !addressPanelIsVisible;

            return addressPanelIsVisible;
        }

        #region Button commands
        #region Button click handlers
        private async void SendButton_Clicked()
        {
            logger.LogInformation("--- MapPage - start SendButton_Clicked");

            if (!NodesViewModel.HasRoute())
            {
                await mapPageView.DisplayAlert("Alert", "Please create a route before pressing Send.", "OK");
                return;
            }

            sendButtonCanbePressed = false;
            DisableOtherTabs();

            ProgressMessage = "Looking for Casio GPR-B1000 device. Please connect your watch.";
            await bluetoothConnectorService.FindAndConnectToWatch((message) => ProgressMessage = message,
                async (connection) =>
                {
                    logger.LogDebug("Map tab - Device Connection was successful");
                    ProgressMessage = "Connected to GPR-B1000 watch.";

                    MapPageDataConverter mapPageDataConverter = new MapPageDataConverter(NodesViewModel, loggerFactory);

                    var watchDataSenderService = new WatchDataSenderService(connection, mapPageDataConverter.GetDataByteArray(),
                        mapPageDataConverter.GetHeaderByteArray(), loggerFactory);

                    watchDataSenderService.ProgressChanged += WatchDataSenderService_ProgressChanged;
                    await watchDataSenderService.SendRoute();

                    logger.LogDebug("Map tab - after awaiting SendRoute()");
                    DisconnectButtonIsVisible = false;
                    ProgressBarIsVisible = false;

                    return true;
                },
                async () =>
                {
                    ProgressMessage = "An error occured during sending watch commands. Please try to connect again";
                    return true;
                },
                () => DisconnectButtonIsVisible = true);

            EnableOtherTabs();
            sendButtonCanbePressed = true;
        }

        private void DeleteNodeButton_Clicked()
        {
            deleteButtonCanbePressed = false;
            NodesViewModel.DeleteSelectedNode();
            mapPageView.RemoveSelectedPin();
            mapPageView.AddLinesBetweenPinsAsLayer();
            ProgressMessage = "Successfully deleted node.";
            deleteButtonCanbePressed = true;
        }

        private void SelectNodeButton_Clicked()
        {
            selectButtonCanbePressed = false;
            NodesViewModel.ClickOnSelectNode();
            selectButtonCanbePressed = true;
        }

        private async void AddressButton_Clicked()
        {
            addressButtonCanbePressed = false;
            var addressPanelIsVisible = ToggleAddressPanelVisibility();

            if (addressPanelIsVisible)
            {
                try
                {
                    await AddressPanelViewModel.UpdateUserPositionAsync();
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "Error occured during setting GPS position");
                    ProgressMessage = "An error occured during getting your current GPS position. Are you sure you have internet connection ?";
                }
            }

            addressButtonCanbePressed = true;
        }

        private async void DisconnectButton_Clicked()
        {
            disconnectButtonCanbePressed = false;
            await bluetoothConnectorService.DisconnectFromWatch((m) => ProgressMessage = m);
            DisconnectButtonIsVisible = false;
            disconnectButtonCanbePressed = true;
        }
        #endregion

        #region Helper methods
        private void WatchDataSenderService_ProgressChanged(object sender, DataSenderProgressEventArgs e)
        {
            ProgressBarIsVisible = true;
            ProgressMessage = e.Text;
            ProgressBarPercentageMessage = e.PercentageText;
            ProgressBarPercentageNumber = e.PercentageNumber;

            logger.LogDebug($"Current progress bar percentage number: {ProgressBarPercentageNumber}");
        }

        private void EnableOtherTabs()
        {
            appShellViewModel.ConfigPageIsEnabled = true;
            appShellViewModel.DownloadPageIsEnabled = true;
        }

        private void DisableOtherTabs()
        {
            appShellViewModel.ConfigPageIsEnabled = false;
            appShellViewModel.DownloadPageIsEnabled = false;
        }
        #endregion

        #endregion

        #region Properties

        public bool ProgressBarIsVisible { get => progressBarIsVisible; set { progressBarIsVisible = value; OnPropertyChanged("ProgressBarIsVisible"); } }
        public string ProgressBarPercentageMessage { get => progressBarPercentageMessage; set { progressBarPercentageMessage = value; OnPropertyChanged("ProgressBarPercentageMessage"); } }
        public double ProgressBarPercentageNumber { get => progressBarPercentageNumber; set { progressBarPercentageNumber = value; OnPropertyChanged("ProgressBarPercentageNumber"); } }
        public string ProgressMessage { get => progressMessage; set { progressMessage = value; OnPropertyChanged("ProgressMessage"); } }
        public NodesViewModel NodesViewModel { get => nodesViewModel; }
        public RowDefinitionCollection GridViewRows { get => gridViewRows ; set { gridViewRows = value; OnPropertyChanged("GridViewRows"); } }
        public AddressPanelViewModel AddressPanelViewModel { get => addressPanelViewModel; }
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

        #region Button Commands
        public ICommand SendCommand
        {
            get
            {
                if (sendCommand == null)
                {
                    sendCommand = new Command((o) => SendButton_Clicked(), (o) => sendButtonCanbePressed);
                }

                return sendCommand;
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new Command((o) => DeleteNodeButton_Clicked(), (o) => deleteButtonCanbePressed);
                }

                return deleteCommand;
            }
        }

        public ICommand SelectCommand
        {
            get
            {
                if (selectCommand == null)
                {
                    selectCommand = new Command((o) => SelectNodeButton_Clicked(), (o) => selectButtonCanbePressed);
                }

                return selectCommand;
            }
        }

        public ICommand AddressCommand
        {
            get
            {
                if (addressCommand == null)
                {
                    addressCommand = new Command((o) => AddressButton_Clicked(), (o) => addressButtonCanbePressed);
                }

                return addressCommand;
            }
        }

        public ICommand DisconnectCommand
        {
            get
            {
                if (disconnectCommand == null)
                {
                    disconnectCommand = new Command((o) => DisconnectButton_Clicked(), (o) => disconnectButtonCanbePressed);
                }

                return disconnectCommand;
            }
        }

        #endregion

        public bool ShowCalculatedDistances
        {
            get
            {
                return showCalculatedDistance;
            }
            set
            {
                showCalculatedDistance = value;
            }
        }
        #endregion
    }
}
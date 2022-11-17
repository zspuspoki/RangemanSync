using Android.Content;
using Rangeman.Views.Map;
using Xamarin.Forms;
using Rangeman.Services.BluetoothConnector;
using Rangeman.WatchDataSender;
using System.Diagnostics;
using System.Windows.Input;

namespace Rangeman
{
    public class MapPageViewModel : ViewModelBase
    {
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
        private RowDefinitionCollection gridViewRows;
        private bool watchCommandButtonsAreVisible = true;
        private bool disconnectButtonIsVisible = false;

        private bool sendButtonCanbePressed = true;
        private bool deleteButtonCanbePressed = true;
        private bool selectButtonCanbePressed = true;
        private bool addressButtonCanbePressed = true;
        private bool disconnectButtonCanbePressed = true;

        public MapPageViewModel(Context context, NodesViewModel nodesViewModel, 
            AddressPanelViewModel addressPanelViewModel, IMapPageView mapPageView, 
            AppShellViewModel appShellViewModel, BluetoothConnectorService bluetoothConnectorService)
        {
            Context = context;
            this.nodesViewModel = nodesViewModel;
            this.addressPanelViewModel = addressPanelViewModel;
            this.mapPageView = mapPageView;
            this.appShellViewModel = appShellViewModel;
            this.bluetoothConnectorService = bluetoothConnectorService;
            gridViewRows = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = 0 }
            };
        }

       

        public void UpdateMapToUseMbTilesFile()
        {
            mapPageView.UpdateMapToUseMbTilesFile();            
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
            Debug.WriteLine("--- MapPage - start SendButton_Clicked");

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
                    Debug.WriteLine("Map tab - Device Connection was successful");
                    ProgressMessage = "Connected to GPR-B1000 watch.";

                    MapPageDataConverter mapPageDataConverter = new MapPageDataConverter(NodesViewModel);

                    var watchDataSenderService = new WatchDataSenderService(connection, mapPageDataConverter.GetDataByteArray(),
                        mapPageDataConverter.GetHeaderByteArray());

                    watchDataSenderService.ProgressChanged += WatchDataSenderService_ProgressChanged;
                    await watchDataSenderService.SendRoute();

                    Debug.WriteLine("Map tab - after awaiting SendRoute()");
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
            NodesViewModel.DeleteSelectedNode();
            mapPageView.RemoveSelectedPin();
            mapPageView.AddLinesBetweenPinsAsLayer();
            ProgressMessage = "Successfully deleted node.";
        }

        private void SelectNodeButton_Clicked()
        {
            NodesViewModel.ClickOnSelectNode();
        }

        private async void AddressButton_Clicked()
        {
            var addressPanelIsVisible = ToggleAddressPanelVisibility();

            if (addressPanelIsVisible)
            {
                await AddressPanelViewModel.UpdateUserPositionAsync();
            }
        }

        private async void DisconnectButton_Clicked()
        {
            await bluetoothConnectorService.DisconnectFromWatch((m) => ProgressMessage = m);
            DisconnectButtonIsVisible = false;
        }
        #endregion

        #region Helper methods
        private void WatchDataSenderService_ProgressChanged(object sender, DataSenderProgressEventArgs e)
        {
            ProgressBarIsVisible = true;
            ProgressMessage = e.Text;
            ProgressBarPercentageMessage = e.PercentageText;
            ProgressBarPercentageNumber = e.PercentageNumber;

            Debug.WriteLine($"Current progress bar percentage number: {ProgressBarPercentageNumber}");
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
        public Context Context { get; }
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
                if (applyCommand == null)
                {
                    applyCommand = new Command((o) => ApplySettings(), (o) => CanApplySettings());
                }

                return applyCommand;
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                if (applyCommand == null)
                {
                    applyCommand = new Command((o) => ApplySettings(), (o) => CanApplySettings());
                }

                return applyCommand;
            }
        }

        public ICommand SelectCommand
        {
            get
            {
                if (applyCommand == null)
                {
                    applyCommand = new Command((o) => ApplySettings(), (o) => CanApplySettings());
                }

                return applyCommand;
            }
        }

        public ICommand AddressCommand
        {
            get
            {
                if (applyCommand == null)
                {
                    applyCommand = new Command((o) => ApplySettings(), (o) => CanApplySettings());
                }

                return applyCommand;
            }
        }

        public ICommand DisconnectCommand
        {
            get
            {
                if (applyCommand == null)
                {
                    applyCommand = new Command((o) => ApplySettings(), (o) => CanApplySettings());
                }

                return applyCommand;
            }
        }

        #endregion
        #endregion
    }
}
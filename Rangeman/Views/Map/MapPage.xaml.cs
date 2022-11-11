using Mapsui.UI.Forms;
using nexus.protocols.ble;
using Rangeman.Services.BluetoothConnector;
using Rangeman.WatchDataSender;
using System;
using System.Diagnostics;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rangeman
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        private IBluetoothLowEnergyAdapter ble;
        private MapPageViewModel viewModel = null;
        private BluetoothConnectorService bluetoothConnectorService;

        public MapPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            this.BindingContextChanged += MapPage_BindingContextChanged;
        }

        private void MapPage_BindingContextChanged(object sender, EventArgs e)
        {
            var vm = this.BindingContext as MapPageViewModel;
            if (vm != null)
            {
                if (viewModel == null)
                {
                    viewModel = vm;
                    viewModel.AddressPanelViewModel.PlaceOnMapClicked += AddressPanelViewModel_PlaceOnMapClicked;
                }

                ble = BluetoothLowEnergyAdapter.ObtainDefaultAdapter(vm.Context);
                this.bluetoothConnectorService = new BluetoothConnectorService(ble);
            }
        }

        private void AddressPanelViewModel_PlaceOnMapClicked(object sender, Position p)
        {
            var pinTitle = GetPinTitle(p.Longitude, p.Latitude);

            if (pinTitle == null)
            {
                return;
            }

            ShowPinOnMap(pinTitle, p, true);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is MapPageViewModel mapPageViewModel)
            {
                mapView.Map = mapPageViewModel.Map;
            }

            var location = await Geolocation.GetLastKnownLocationAsync();

            if (location != null)
            {
                Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
            }

            mapView.MyLocationLayer.UpdateMyLocation(new Position(location.Latitude, location.Longitude), true);
        }

        private void mapView_MapClicked(object sender, MapClickedEventArgs e)
        {
            var pinTitle = GetPinTitle(e.Point.Longitude, e.Point.Latitude);

            if (pinTitle == null)
            {
                return;
            }

            ShowPinOnMap(pinTitle, e.Point);
        }

        private string GetPinTitle(double longitude, double latitude)
        {
            var pinTitle = viewModel.NodesViewModel.AddNodeToMap(longitude, latitude);
            return pinTitle;
        }

        private void ShowPinOnMap(string pinTitle, Position p, bool setTitleImmediately = false)
        {
            var pin = new Pin(mapView)
            {
                Label = "unset",
                Position = p,
                RotateWithMap = false
            };

            pin.Callout.Type = Mapsui.Rendering.Skia.CalloutType.Single;
            
            if(setTitleImmediately)
            {
                pin.Callout.Title = pinTitle;
            }

            pin.Callout.CalloutClicked += (s, e) =>
            {
                Debug.WriteLine($"Map page: entering callout clicked");
                if (e.Callout.Title == "unset")
                {
                    Debug.WriteLine($"Map page: callout clicked. Number of taps: {e.NumOfTaps}");
                    pin.Callout.Title = pinTitle;
                    e.Handled = true;
                    e.Callout.Type = Mapsui.Rendering.Skia.CalloutType.Single;
                }
                else
                {
                    if (BindingContext is MapPageViewModel mapPageViewModel)
                    {
                        Debug.WriteLine("Map: Callout click - other");
                        e.Handled = true;
                        var pinTitle = e.Callout.Title;
                        mapPageViewModel.NodesViewModel.SelectNodeForDeletion(pinTitle, e.Point.Longitude, e.Point.Latitude);
                        mapPageViewModel.ProgressMessage = $"Selected node: {pinTitle} Please use the delete button to delete it.";
                        mapView.SelectedPin = e.Callout.Pin;
                    }
                }
            };

            mapView.Pins.Add(pin);
            pin.ShowCallout();
            viewModel.AddLinesBetweenPinsAsLayer();
        }

        private async void SendButton_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("--- MapPage - start SendButton_Clicked");

            if (!viewModel.NodesViewModel.HasRoute())
            {
                await DisplayAlert("Alert", "Please create a route before pressing Send.", "OK");
                return;
            }

            SendButton.Clicked -= SendButton_Clicked;

            await bluetoothConnectorService.FindAndConnectToWatch((message) => viewModel.ProgressMessage = message, 
                async (connection) => 
                {
                    Debug.WriteLine("Map tab - Device Connection was successful");
                    viewModel.ProgressMessage = "Connected to GPR-B1000 watch.";

                    MapPageDataConverter mapPageDataConverter = new MapPageDataConverter(viewModel.NodesViewModel);

                    var watchDataSenderService = new WatchDataSenderService(connection, mapPageDataConverter.GetDataByteArray(),
                        mapPageDataConverter.GetHeaderByteArray());

                    watchDataSenderService.ProgressChanged += WatchDataSenderService_ProgressChanged;
                    await watchDataSenderService.SendRoute();

                    Debug.WriteLine("Map tab - after awaiting SendRoute()");
                });

            SendButton.Clicked += SendButton_Clicked;
        }

        private void WatchDataSenderService_ProgressChanged(object sender, DataSenderProgressEventArgs e)
        {
            if (sender is WatchDataSenderService)
            {
                viewModel.ProgressBarIsVisible = true;
                viewModel.ProgressMessage = e.Text;
                viewModel.ProgressBarPercentageMessage = e.PercentageText;
                viewModel.ProgressBarPercentageNumber = e.PercentageNumber;

                Debug.WriteLine($"Current progress bar percentage number: {viewModel.ProgressBarPercentageNumber}");
            }
        }

        private void DeleteNodeButton_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is MapPageViewModel mapPageViewModel)
            {
                mapPageViewModel.NodesViewModel.DeleteSelectedNode();
                mapView.Pins.Remove(mapView.SelectedPin);
                viewModel.AddLinesBetweenPinsAsLayer();
                mapPageViewModel.ProgressMessage = "Successfully deleted node.";
            }
        }

        private void SelectNodeButton_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is MapPageViewModel mapPageViewModel)
            {
                mapPageViewModel.NodesViewModel.ClickOnSelectNode();
            }
        }

        private async void AddressButton_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is MapPageViewModel mapPageViewModel)
            {
                var addressPanelIsVisible = mapPageViewModel.ToggleAddressPanelVisibility();

                if(addressPanelIsVisible)
                {
                    await mapPageViewModel.AddressPanelViewModel.UpdateUserPositionAsync();
                }
            }
        }
    }
}
using Mapsui.UI.Forms;
using nexus.protocols.ble;
using Rangeman.WatchDataSender;
using System;
using System.Diagnostics;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rangeman
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        private IBluetoothLowEnergyAdapter ble;
        private CancellationTokenSource scanCancellationTokenSource = new CancellationTokenSource();
        private MapPageViewModel viewModel = null;
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

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
                viewModel = vm;
                ble = BluetoothLowEnergyAdapter.ObtainDefaultAdapter(vm.Context);
            }
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
            string pinTitle = null;

            if (BindingContext is MapPageViewModel mapPageViewModel)
            {
                pinTitle = mapPageViewModel.NodesViewModel.AddNodeToMap(e.Point.Longitude, e.Point.Latitude);
            }

            if (pinTitle == null)
            {
                return;
            }

            ShowPinOnMap(pinTitle, e);
        }

        private void ShowPinOnMap(string pinTitle, MapClickedEventArgs e)
        {
            var pin = new Pin(mapView)
            {
                Label = "Test",
                Position = e.Point,
                RotateWithMap = true
            };
            pin.Callout.Type = Mapsui.Rendering.Skia.CalloutType.Detail;
            pin.Callout.Content = 1;
            pin.Callout.CalloutClicked += async (s, e) =>
            {
                e.Callout.Type = Mapsui.Rendering.Skia.CalloutType.Single;
                e.Callout.Title = pinTitle;
                e.Handled = true;
            };

            mapView.Pins.Add(pin);
            pin.ShowCallout();
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
            await ble.ScanForBroadcasts(async (a) =>
            {
                await semaphoreSlim.WaitAsync();
                try
                {
                    if (a.Advertisement != null)
                    {
                        var advertisedName = a.Advertisement.DeviceName;

                        Debug.WriteLine($"--- MapPage SendButton_Clicked, advertised device name: {advertisedName}");
                        viewModel.ProgressMessage = "Looking for Casio device...";

                        if (advertisedName != null &&
                            advertisedName.Contains("CASIO GPR-B1000")
                            && viewModel.NodesViewModel.HasRoute())
                        {
                            Debug.WriteLine("--- MapPage SendButton_Clicked - advertised name contains CASIO");
                            viewModel.ProgressMessage = "Found Casio GPR-B1000. Starting connection...";

                            scanCancellationTokenSource.Cancel();

                            var connection = await ble.ConnectToDevice(a);

                            if (connection.IsSuccessful())
                            {
                                Debug.WriteLine("Map tab - Device Connection was successful");
                                viewModel.ProgressMessage = "Connected to GPR-B1000 watch.";

                                MapPageDataConverter mapPageDataConverter = new MapPageDataConverter(viewModel.NodesViewModel);

                                var watchDataSenderService = new WatchDataSenderService(connection, mapPageDataConverter.GetDataByteArray(),
                                    mapPageDataConverter.GetHeaderByteArray());

                                watchDataSenderService.ProgressChanged += WatchDataSenderService_ProgressChanged;
                                await watchDataSenderService.SendRoute();

                                Debug.WriteLine("Map tab - after awaiting SendRoute()");
                            }
                            else
                            {
                                Debug.WriteLine("Map tab - Device Connection wasn't successful");
                            }

                            SendButton.Clicked += SendButton_Clicked;
                        }
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }, scanCancellationTokenSource.Token);
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

        private void mapView_PinClicked(object sender, PinClickedEventArgs e)
        {
            var pin = sender as Pin;
            if (pin != null)
            {
                if (BindingContext is MapPageViewModel mapPageViewModel)
                {
                    mapPageViewModel.NodesViewModel.SelectNodeForDeletion(e.Pin.Callout.Title, e.Point.Longitude, e.Point.Latitude);
                    mapView.SelectedPin = e.Pin;
                }
            }
        }

        private void DeleteNodeButton_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is MapPageViewModel mapPageViewModel)
            {
                mapPageViewModel.NodesViewModel.DeleteSelectedNode();
                mapView.Pins.Remove(mapView.SelectedPin);
            }
        }

        private void SelectNodeButton_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is MapPageViewModel mapPageViewModel)
            {
                mapPageViewModel.NodesViewModel.ClickOnSelectNode();
            }
        }
    }
}
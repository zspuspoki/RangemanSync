using Mapsui;
using Mapsui.Projection;
using Mapsui.UI.Forms;
using Mapsui.Utilities;
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

        public MapPage()
        {            
            InitializeComponent();

            this.BindingContextChanged += MapPage_BindingContextChanged;
            InitalizeMap();
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
            var location = await Geolocation.GetLastKnownLocationAsync();

            if (location != null)
            {
                Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
            }

            mapView.MyLocationLayer.UpdateMyLocation(new Position(location.Latitude, location.Longitude), true);
        }

        private async void mapView_MapClicked(object sender, Mapsui.UI.Forms.MapClickedEventArgs e)
        {
            if (viewModel.HasEndCoordinate && viewModel.HasStartCoordinate)
            {
                return;
            }

            if (!viewModel.HasStartCoordinate)
            {
                viewModel.AddStartCoordinates(e.Point.Longitude, e.Point.Latitude);
            }
            else if (!viewModel.HasEndCoordinate)
            {
                viewModel.AddEndCoordinates(e.Point.Longitude, e.Point.Latitude);
            }

            ShowPinOnMap(e);
        }

        private async void InitalizeMap()
        {
            var map = new Mapsui.Map
            {
                CRS = "EPSG:3857",
                Transformation = new MinimalTransformation()
            };

            var tileLayer = OpenStreetMap.CreateTileLayer();

            map.Layers.Add(tileLayer);
            map.Widgets.Add(new Mapsui.Widgets.ScaleBar.ScaleBarWidget(map)
            {
                TextAlignment = Mapsui.Widgets.Alignment.Center,
                HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left,
                VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Bottom
            });

            var location = await Geolocation.GetLocationAsync();
            var smc = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);

            var mapResolutions = map.Resolutions;
            map.Home = n => n.NavigateTo(smc, map.Resolutions[17]);

            mapView.Map = map;
        }

        private void ShowPinOnMap(MapClickedEventArgs e)
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
                e.Callout.Title = !viewModel.HasEndCoordinate ? "S" : "E";
                e.Handled = true;
            };

            mapView.Pins.Add(pin);
            pin.ShowCallout();
        }

        private async void SendButton_Clicked(object sender, EventArgs e)
        {
            await ble.ScanForBroadcasts(async (a) =>
            {
                if (a.Advertisement != null)
                {
                    var advertisedName = a.Advertisement.DeviceName;

                    Debug.WriteLine($"--- MapPage SendButton_Clicked, advertised device name: {advertisedName}");
                    viewModel.ProgressMessage = $"Looking for Casio device. Currently found device name: {advertisedName}";

                    if (advertisedName != null && advertisedName.Contains("CASIO"))
                    {
                        Debug.WriteLine("--- MapPage SendButton_Clicked - advertised name contains CASIO");
                        viewModel.ProgressMessage = "Found Casio GPR-B1000. Starting connection...";

                        scanCancellationTokenSource.Cancel();

                        var connection = await ble.ConnectToDevice(a);

                        if (connection.IsSuccessful())
                        {
                            Debug.WriteLine("Map tab - Device Connection was successful");
                            viewModel.ProgressMessage = "Connected to GPR-B1000 watch.";

                            var watchDataSenderService = new WatchDataSenderService(connection, viewModel.ToTestDataByteArray(), viewModel.ToTestHeaderByteArray());
                            watchDataSenderService.ProgressChanged += WatchDataSenderService_ProgressChanged;
                            await watchDataSenderService.SendRoute();                            
                        }
                        else
                        {
                            Debug.WriteLine("Map tab - Device Connection wasn't successful");
                        }
                    }
                }
            }, scanCancellationTokenSource.Token);
        }

        private void WatchDataSenderService_ProgressChanged(object sender, DataSenderProgressEventArgs e)
        {
            if(sender is WatchDataSenderService)
            {
                viewModel.ProgressBarIsVisible = true;
                viewModel.ProgressMessage = e.Text;
                viewModel.ProgressBarPercentageMessage = e.PercentageText;
                viewModel.ProgressBarPercentageNumber = e.PercentageNumber;
            }
        }
    }
}
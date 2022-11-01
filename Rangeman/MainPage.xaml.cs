using employeeID;
using nexus.protocols.ble;
using nexus.protocols.ble.scan;
using Rangeman.DataExtractors.Data;
using SharpGPX;
using SharpGPX.GPX1_0;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Debug = System.Diagnostics.Debug;

namespace Rangeman
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private const string WatchDeviceName = "CASIO GPR-B1000";
        private IBluetoothLowEnergyAdapter ble;
        private MainPageViewModel viewModel = null;
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            this.BindingContextChanged += MainPage_BindingContextChanged;
        }

        private void MainPage_BindingContextChanged(object sender, EventArgs e)
        {
            var vm = this.BindingContext as MainPageViewModel;
            if(vm != null)
            {
                viewModel = vm;
                ble = BluetoothLowEnergyAdapter.ObtainDefaultAdapter(vm.Context);
            }
        }

        private async void DownloadHeaders_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("--- MainPage - start DownloadHeaders_Clicked");

            DownloadHeadersButton.Clicked -= DownloadHeaders_Clicked;

            CancellationTokenSource scanCancellationTokenSource = new CancellationTokenSource();
            IBlePeripheral device = null;

            await ble.ScanForBroadcasts((a) =>
            {
                if (a.Advertisement != null)
                {
                    var advertisedName = a.Advertisement.DeviceName;

                    Debug.WriteLine($"--- MainPage DownloadHeaders_Clicked, advertised device name: {advertisedName}");

                    if (advertisedName != null &&
                        advertisedName.Contains(WatchDeviceName))
                    {
                        Debug.WriteLine("--- MainPage DownloadHeaders_Clicked - advertised name contains CASIO");

                        device = a;

                        scanCancellationTokenSource.Cancel();
                    }
                }
            }, scanCancellationTokenSource.Token);


            if (device != null)
            {
                var connection = await ble.ConnectToDevice(device);

                if (connection.IsSuccessful())
                {
                    try
                    {
                        var logPointMemoryService = new LogPointMemoryExtractorService(connection);
                        var headers = await logPointMemoryService.GetHeaderDataAsync();
                        headers.ForEach(h => viewModel.LogHeaderList.Add(h.ToViewModel()));
                    }
                    finally
                    {
                        await connection.GattServer.Disconnect();
                    }
                }
            }

            DownloadHeadersButton.Clicked += DownloadHeaders_Clicked;
        }

        private async void DownloadSaveGPXButton_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("--- MainPage - start DownloadSaveGPXButton_Clicked");

            DownloadSaveGPXButton.Clicked -= DownloadSaveGPXButton_Clicked;

            CancellationTokenSource scanCancellationTokenSource = new CancellationTokenSource();
            IBlePeripheral device = null;

            await ble.ScanForBroadcasts((a) =>
            {
                if (a.Advertisement != null)
                {
                    var advertisedName = a.Advertisement.DeviceName;

                    Debug.WriteLine($"--- MainPage DownloadSaveGPXButton_Clicked, advertised device name: {advertisedName}");

                    if (advertisedName != null &&
                        advertisedName.Contains(WatchDeviceName))
                    {
                        Debug.WriteLine("--- MainPage DownloadSaveGPXButton_Clicked - advertised name contains CASIO");

                        device = a;

                        scanCancellationTokenSource.Cancel();
                    }
                }
            }, scanCancellationTokenSource.Token);

            if (device != null)
            {
                Debug.WriteLine("DownloadSaveGPXButton_Clicked : Before connecting");
                var connection = await ble.ConnectToDevice(device);
                Debug.WriteLine("DownloadSaveGPXButton_Clicked : After connecting");

                if (connection.IsSuccessful())
                {
                    Debug.WriteLine("DownloadSaveGPXButton_Clicked : Successful connection");

                    try
                    {
                        if (viewModel.SelectedLogHeader != null)
                        {
                            Debug.WriteLine("DownloadSaveGPXButton_Clicked : Before GetLogDataAsync");
                            Debug.WriteLine($"Selected ordinal number: {viewModel.SelectedLogHeader.OrdinalNumber}");
                            var logPointMemoryService = new LogPointMemoryExtractorService(connection);
                            var logDataEntries = await logPointMemoryService.GetLogDataAsync(viewModel.SelectedLogHeader.OrdinalNumber);

                            SaveGPXFile(logDataEntries);
                        }
                        else
                        {
                            Debug.WriteLine("DownloadSaveGPXButton_Clicked : One log header entry should be selected");
                        }
                    }
                    finally
                    {
                        await connection.GattServer.Disconnect();
                    }
                }
            }

            DownloadSaveGPXButton.Clicked += DownloadSaveGPXButton_Clicked;
            //Save selected log header as GPX
        }

        private void SaveGPXFile(List<LogData> logDataEntries)
        {
            GpxClass gpx = new GpxClass();
            gpx.Tracks.Add(new SharpGPX.GPX1_1.trkType());
            gpx.Tracks[0].trkseg.Add(new SharpGPX.GPX1_1.trksegType());

            foreach (var logEntry in logDataEntries)
            {
                var wpt = new SharpGPX.GPX1_1.wptType
                {
                    lat = (decimal)logEntry.Latitude,
                    lon = (decimal)logEntry.Longitude   // ele tag : pressure -> elevation conversion ?
                };
            }

            var headerTime = viewModel.SelectedLogHeader.HeaderTime;
            gpx.ToFile($"GPR-B1000-Route-{headerTime.Year}-{headerTime.Month}-{headerTime.Day}");
        }

        private void LogHeadersList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is LogHeaderViewModel selectedLogHeader)
            {
                viewModel.SelectedLogHeader = selectedLogHeader;
            }
        }
    }
}
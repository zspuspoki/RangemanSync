using employeeID;
using nexus.protocols.ble;
using SharpGPX;
using SharpGPX.GPX1_0;
using System;
using System.Threading;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Debug = System.Diagnostics.Debug;

namespace Rangeman
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private IBluetoothLowEnergyAdapter ble;
        private LogPointMemoryExtractorService logPointMemoryService = null;
        private CancellationTokenSource scanCancellationTokenSource = new CancellationTokenSource();
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

            await ble.ScanForBroadcasts(async (a) =>
            {
                await semaphoreSlim.WaitAsync();
                try
                {
                    if (a.Advertisement != null)
                    {
                        var advertisedName = a.Advertisement.DeviceName;

                        Debug.WriteLine($"--- MainPage DownloadHeaders_Clicked, advertised device name: {advertisedName}");

                        if (advertisedName != null &&
                            advertisedName.Contains("CASIO GPR-B1000"))
                        {
                            Debug.WriteLine("--- MainPage DownloadHeaders_Clicked - advertised name contains CASIO");

                            scanCancellationTokenSource.Cancel();

                            var connection = await ble.ConnectToDevice(a);

                            if (connection.IsSuccessful())
                            {
                                logPointMemoryService = new LogPointMemoryExtractorService(connection);
                                var headers = await logPointMemoryService.GetHeaderDataAsync();
                                headers.ForEach(h => viewModel.LogHeaderList.Add(h.ToViewModel()));
                            }
                            else
                            {

                            }

                            DownloadHeadersButton.Clicked += DownloadHeaders_Clicked;
                        }
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }, scanCancellationTokenSource.Token);
        }

        private async void DownloadSaveGPXButton_Clicked(object sender, EventArgs e)
        {
            if(logPointMemoryService != null)
            {
                if (viewModel.SelectedLogHeader != null)
                {
                    var logDataEntries = await logPointMemoryService.GetLogDataAsync(viewModel.SelectedLogHeader.OrdinalNumber);
                    
                    GpxClass gpx = new GpxClass();
                    gpx.Tracks.Add(new SharpGPX.GPX1_1.trkType());
                    gpx.Tracks[0].trkseg.Add(new SharpGPX.GPX1_1.trksegType());

                    foreach(var logEntry in logDataEntries)
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
                else
                {
                    Debug.WriteLine("DownloadSaveGPXButton_Clicked : One log header entry should be selected");
                }
            }
            //Save selected log header as GPX
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
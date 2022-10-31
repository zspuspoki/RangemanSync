using employeeID;
using nexus.protocols.ble;
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
        private LogPointMemoryExtractorService dataExtractor = null;
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
                                var logPointMemoryService = new LogPointMemoryExtractorService(connection);
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

        private void DownloadSaveGPXButton_Clicked(object sender, EventArgs e)
        {
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
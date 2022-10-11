using employeeID;
using nexus.protocols.ble;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public MainPage()
        {
            InitializeComponent();
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

        private async void searchDevice(object sender, EventArgs e)
        {
            await ble.ScanForBroadcasts((a) =>
            {
                if (a.Advertisement != null)
                {
                    var advertisedName = a.Advertisement.DeviceName;

                    if (advertisedName != null && advertisedName.Contains("CASIO") && !viewModel.DeviceList.Any(e => e.Name == advertisedName))
                    {
                        viewModel.DeviceList.Add(new ListItem { Name = advertisedName, Device = a });
                    }
                }
            },scanCancellationTokenSource.Token);
        }

        private async void DevicesList_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            scanCancellationTokenSource.Cancel();

            var deviceListItem = DevicesList.SelectedItem as ListItem;

            if (deviceListItem.Device != null)
            {
                var connection = await ble.ConnectToDevice(deviceListItem.Device);

                if (connection.IsSuccessful())
                {
                    Debug.WriteLine("Connection was successful");
                    dataExtractor = new LogPointMemoryExtractorService(connection);
                    dataExtractor.AllLogDataReceived += DataExtractor_AllLogDataReceived;
                    dataExtractor.StartDownloadLogAndPointMemoryData();
                }
                else
                {
                    Debug.WriteLine("Connection wasn't successful");
                }
            }
        }

        private void DataExtractor_AllLogDataReceived(object sender, List<DataExtractors.Data.LogData> logDataItems)
        {
            viewModel.DeviceList.Clear();
            foreach(var logData in logDataItems)
            {
                var listItemText = $"Date = {logData.Date}, Latitude = {logData.Latitude}, \n Longitude = {logData.Longitude}, Pressure = {logData.Pressure},\n Temperature = {logData.Temperature}";
                viewModel.DeviceList.Add(new ListItem { Name = listItemText, Device = null });
            }
        }
    }
}
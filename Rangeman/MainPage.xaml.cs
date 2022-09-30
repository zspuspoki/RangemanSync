using employeeID;
using nexus.protocols.ble;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        ObservableCollection<ListItem> devicelist;
        IBluetoothLowEnergyAdapter ble;
        LogPointMemoryExtractor dataExtractor = null;
        CancellationTokenSource scanCancellationTokenSource = new CancellationTokenSource();

        public MainPage(Android.Content.Context context)
        {
            InitializeComponent();

            ble = BluetoothLowEnergyAdapter.ObtainDefaultAdapter(context);

            devicelist = new ObservableCollection<ListItem>();
            DevicesList.ItemsSource = devicelist;
        }

        private async void searchDevice(object sender, EventArgs e)
        {
            await ble.ScanForBroadcasts((a) =>
            {
                if (a.Advertisement != null)
                {
                    var advertisedName = a.Advertisement.DeviceName;

                    if (advertisedName != null && advertisedName.Contains("CASIO") && !devicelist.Any(e => e.Name == advertisedName))
                    {
                        devicelist.Add(new ListItem { Name = advertisedName, Device = a });
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
                    dataExtractor = new LogPointMemoryExtractor(connection);
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
            devicelist.Clear();
            foreach(var logData in logDataItems)
            {
                var listItemText = $"Date = {logData.Date}, Latitude = {logData.Latitude}, \n Longitude = {logData.Longitude}, Pressure = {logData.Pressure},\n Temperature = {logData.Temperature}";
                devicelist.Add(new ListItem { Name = listItemText, Device = null });
            }
        }
    }
}
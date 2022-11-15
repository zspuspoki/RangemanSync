﻿using employeeID;
using Microsoft.Extensions.Logging;
using Rangeman.DataExtractors.Data;
using Rangeman.Services.BluetoothConnector;
using SharpGPX;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Debug = System.Diagnostics.Debug;

namespace Rangeman
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private BluetoothConnectorService bluetoothConnectorService;
        private readonly ILogger<MainPage> logger;

        public MainPage(BluetoothConnectorService bluetoothConnectorService, ILogger<MainPage> logger)
        {
            InitializeComponent();
            this.bluetoothConnectorService = bluetoothConnectorService;
            this.logger = logger;

            logger.LogInformation("MainPage instatiated");
        }

        private async void DownloadHeaders_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("--- MainPage - start DownloadHeaders_Clicked");
            SetProgressMessage("Looking for Casio GPR-B1000 device. Please connect your watch.");

            DownloadHeadersButton.Clicked -= DownloadHeaders_Clicked;

            await bluetoothConnectorService.FindAndConnectToWatch(SetProgressMessage, async (connection) => 
            {
                var logPointMemoryService = new LogPointMemoryExtractorService(connection);
                logPointMemoryService.ProgressChanged += LogPointMemoryService_ProgressChanged;
                var headers = await logPointMemoryService.GetHeaderDataAsync();
                headers.ForEach(h => ViewModel.LogHeaderList.Add(h.ToViewModel()));

                logPointMemoryService.ProgressChanged -= LogPointMemoryService_ProgressChanged;

                return true;
            },
            async ()=> 
            {
                SetProgressMessage("An error occured during sending watch commands. Please try to connect again");
                return true;
            });

            DownloadHeadersButton.Clicked += DownloadHeaders_Clicked;
        }

        private async void DownloadSaveGPXButton_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("--- MainPage - start DownloadSaveGPXButton_Clicked");
            SetProgressMessage("Looking for Casio GPR-B1000 device. Please connect your watch.");

            DownloadSaveGPXButton.Clicked -= DownloadSaveGPXButton_Clicked;

            await bluetoothConnectorService.FindAndConnectToWatch(SetProgressMessage, async (connection) =>
            {
                if (ViewModel.SelectedLogHeader != null)
                {
                    Debug.WriteLine("DownloadSaveGPXButton_Clicked : Before GetLogDataAsync");
                    Debug.WriteLine($"Selected ordinal number: {ViewModel.SelectedLogHeader.OrdinalNumber}");
                    var logPointMemoryService = new LogPointMemoryExtractorService(connection);
                    logPointMemoryService.ProgressChanged += LogPointMemoryService_ProgressChanged;
                    var selectedHeader = ViewModel.SelectedLogHeader;
                    var logDataEntries = await logPointMemoryService.GetLogDataAsync(selectedHeader.OrdinalNumber,
                        selectedHeader.DataSize,
                        selectedHeader.DataCount,
                        selectedHeader.LogAddress,
                        selectedHeader.LogTotalLength);

                    logPointMemoryService.ProgressChanged -= LogPointMemoryService_ProgressChanged;

                    SaveGPXFile(logDataEntries);

                    return true;
                }
                else
                {
                    Debug.WriteLine("DownloadSaveGPXButton_Clicked : One log header entry should be selected");
                    return false;
                }
            },
            async () =>
            {
                SetProgressMessage("An error occured during sending watch commands. Please try to connect again");
                return true;
            });

            DownloadSaveGPXButton.Clicked += DownloadSaveGPXButton_Clicked;
            //Save selected log header as GPX
        }

        private void LogPointMemoryService_ProgressChanged(object sender, WatchDataReceiver.DataReceiverProgressEventArgs e)
        {
            SetProgressMessage(e.Text);
        }

        private async void SaveGPXFile(List<LogData> logDataEntries)
        {
            GpxClass gpx = new GpxClass();
            
            gpx.Metadata.time = ViewModel.SelectedLogHeader.HeaderTime;
            gpx.Metadata.timeSpecified = true;
            gpx.Metadata.desc = "Track exported from Casio GPR-B1000 watch";

            gpx.Tracks.Add(new SharpGPX.GPX1_1.trkType());
            gpx.Tracks[0].trkseg.Add(new SharpGPX.GPX1_1.trksegType());

            foreach (var logEntry in logDataEntries)
            {
                var wpt = new SharpGPX.GPX1_1.wptType
                {
                    lat = (decimal)logEntry.Latitude,
                    lon = (decimal)logEntry.Longitude,   // ele tag : pressure -> elevation conversion ?
                    time = logEntry.Date,
                    timeSpecified = true,
                };

                gpx.Tracks[0].trkseg[0].trkpt.Add(wpt);
            }

            var headerTime = ViewModel.SelectedLogHeader.HeaderTime;
            var fileName = $"GPR-B1000-Route-{headerTime.Year}-{headerTime.Month}-{headerTime.Day}-2.gpx";
            var path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            string filePath = System.IO.Path.Combine(path, fileName);
            gpx.ToFile(filePath);
            await DisplayAlert("Alert", $"File saved here: {filePath}", "OK");
        }

        private void LogHeadersList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is LogHeaderViewModel selectedLogHeader)
            {
                ViewModel.SelectedLogHeader = selectedLogHeader;
            }
        }

        private void SetProgressMessage(string message)
        {
            ViewModel.ProgressMessage = message;
        }

        public MainPageViewModel ViewModel
        {
            get
            {
                var vm = BindingContext as MainPageViewModel;
                return vm;
            }
        }
    }
}
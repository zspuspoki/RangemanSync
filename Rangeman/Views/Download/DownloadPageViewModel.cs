﻿using Android.Content;
using Microsoft.Extensions.Logging;
using Rangeman.DataExtractors.Data;
using Rangeman.Services.BluetoothConnector;
using Rangeman.Services.LicenseDistributor;
using Rangeman.Services.SharedPreferences;
using Rangeman.Services.WatchDataReceiver;
using Rangeman.Views.Download;
using SharpGPX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace Rangeman
{
    public class DownloadPageViewModel : ViewModelBase
    {
        private string progressMessage = "Press the download headers button to start downloading the previously recorded routes from the watch. This message can be closed any time by tapping on it";
        private bool watchCommandButtonsAreVisible = true;
        private bool disconnectButtonIsVisible = false;
        private ILogger<DownloadPageViewModel> logger;
        private readonly BluetoothConnectorService bluetoothConnectorService;
        private readonly AppShellViewModel appShellViewModel;
        private readonly IDownloadPageView downloadPageView;
        private readonly ILoggerFactory loggerFactory;
        private readonly ISharedPreferencesService sharedPreferencesService;
        private readonly ISaveGPXFileService saveGPXFileService;
        private bool disconnectButtonCanBePressed = true;
        private bool downloadHeadersButtonCanBePressed = true;
        private bool saveGPXButtonCanBePressed = true;
        private DateTime lastHeaderDownloadTime;
        private bool hasValidLicense = true;

        private ICommand disconnectCommand;
        private ICommand downloadHeadersCommand;
        private ICommand saveGPXCommand;

        public DownloadPageViewModel(Context context, BluetoothConnectorService bluetoothConnectorService, 
            AppShellViewModel appShellViewModel, IDownloadPageView downloadPageView,
            ILoggerFactory loggerFactory, ISharedPreferencesService sharedPreferencesService,
            ISaveGPXFileService saveGPXFileService, ILicenseDistributor licenseDistributor)
        {
            this.logger = loggerFactory.CreateLogger<DownloadPageViewModel>();

            logger.LogInformation("Inside Download page VM ctor");

            this.bluetoothConnectorService = bluetoothConnectorService;
            this.appShellViewModel = appShellViewModel;
            this.downloadPageView = downloadPageView;
            this.loggerFactory = loggerFactory;
            this.sharedPreferencesService = sharedPreferencesService;
            this.saveGPXFileService = saveGPXFileService;

            HandleLicenseResponse(licenseDistributor);
            HandleLicenseErrorResponse(licenseDistributor);

            MessagingCenter.Subscribe<ILicenseDistributor>(this, DistributorMessages.LicenseResultReceived.ToString(),
                HandleLicenseResponse);

            MessagingCenter.Subscribe<ILicenseDistributor>(this, DistributorMessages.AppErrorReceived.ToString(),
                HandleLicenseErrorResponse);
        }

        #region Button handlers
        private async void DisconnectButton_Clicked()
        {
            disconnectButtonCanBePressed = false;
            await bluetoothConnectorService.DisconnectFromWatch(SetProgressMessage);
            DisconnectButtonIsVisible = false;
            disconnectButtonCanBePressed = true;
        }

        private async void DownloadHeaders_Clicked()
        {
            logger.LogInformation("--- MainPage - start DownloadHeaders_Clicked");

            SetProgressMessage("Looking for Casio GPR-B1000 device. Please connect your watch.");

            downloadHeadersButtonCanBePressed = false;
            DisableOtherTabs();

            await bluetoothConnectorService.FindAndConnectToWatch(SetProgressMessage, async (connection) =>
            {
                var logPointMemoryService = new LogPointMemoryExtractorService(connection, loggerFactory);
                logPointMemoryService.ProgressChanged += LogPointMemoryService_ProgressChanged;
                var headersTask = logPointMemoryService.GetHeaderDataAsync();
                var headers = await headersTask;

                LogHeaderList.Clear();

                if (headers != null && headers.Count > 0)
                {
                    headers.ForEach(h => LogHeaderList.Add(h.ToViewModel()));
                }
                else
                {
                    logger.LogDebug("Headers downloading resulted 0 headers");
                    SetProgressMessage("Headers downloading resulted 0 headers. Please make sure you have recorded routes on the watch. If yes, then please try again because the transmission has been terminated by the watch.");
                }

                logPointMemoryService.ProgressChanged -= LogPointMemoryService_ProgressChanged;

                DisconnectButtonIsVisible = false;
                lastHeaderDownloadTime = DateTime.Now;
                return true;
            },
            async () =>
            {
                SetProgressMessage("An error occured during sending watch commands. Please try to connect again");
                return true;
            },
            () => DisconnectButtonIsVisible = true);

            EnableOtherTabs();
            downloadHeadersButtonCanBePressed = true;
        }

        private async void DownloadSaveGPXButton_Clicked()
        {
            logger.LogInformation("--- MainPage - start DownloadSaveGPXButton_Clicked");

            if (!hasValidLicense)
            {
                ProgressMessage = "Invalid license detected : downloading is not allowed.";
                return;
            }

            if (SelectedLogHeader == null)
            {
                logger.LogDebug("DownloadSaveGPXButton_Clicked : One log header entry should be selected");
                SetProgressMessage("Please select a log header from the list or start downloading the list by using the download headers button if you haven't done it yet.");
                return;
            }

            var timeElapsedSinceLastHeaderDownloadTime = DateTime.Now - lastHeaderDownloadTime;

            if(timeElapsedSinceLastHeaderDownloadTime.TotalMinutes > 30)
            {
                logger.LogDebug($"--- Old header data detected. Elapsed minutes = {timeElapsedSinceLastHeaderDownloadTime}");
                SetProgressMessage("The header data is more than 30 minutes old. Please download the headers again by pressing the Download headers button.");
                return;
            }

            SetProgressMessage("Looking for Casio GPR-B1000 device. Please connect your watch.");

            saveGPXButtonCanBePressed = false;
            DisableOtherTabs();

            await bluetoothConnectorService.FindAndConnectToWatch(SetProgressMessage, async (connection) =>
            {
                logger.LogDebug("DownloadSaveGPXButton_Clicked : Before GetLogDataAsync");
                logger.LogDebug($"Selected ordinal number: {SelectedLogHeader.OrdinalNumber}");
                var logPointMemoryService = new LogPointMemoryExtractorService(connection, loggerFactory);
                logPointMemoryService.ProgressChanged += LogPointMemoryService_ProgressChanged;
                var selectedHeader = SelectedLogHeader;
                var logDataEntries = await logPointMemoryService.GetLogDataAsync(
                    selectedHeader.DataSize,
                    selectedHeader.DataCount,
                    selectedHeader.LogAddress,
                    selectedHeader.LogTotalLength);

                logPointMemoryService.ProgressChanged -= LogPointMemoryService_ProgressChanged;

                if (logDataEntries!= null)
                {
                    logger.LogDebug("-- Inside DownloadSaveAsGPXButton: logDataEntries is not null! Calling SaveGPXFile()");
                    SaveGPXFile(logDataEntries);
                }
                else
                {
                    ProgressMessage = "The data downloading from the watch has been ended without receiving all of the data including the end transmission command. Please try again by pressing the download as GPX button again.";
                    logger.LogDebug("-- Inside DownloadSaveAsGPXButton: logDataEntries is null");
                }

                DisconnectButtonIsVisible = false;

                return true;
            },
            async () =>
            {
                SetProgressMessage("An error occured during sending watch commands. Please try to connect again");
                return true;
            },
            () => DisconnectButtonIsVisible = true);

            EnableOtherTabs();
            saveGPXButtonCanBePressed = true;
            //Save selected log header as GPX
        }

        private void DisableOtherTabs()
        {
            appShellViewModel.ConfigPageIsEnabled = false;
            appShellViewModel.MapPageIsEnabled = false;
        }

        private void EnableOtherTabs()
        {
            appShellViewModel.ConfigPageIsEnabled = true;
            appShellViewModel.MapPageIsEnabled = true;
        }

        private void LogPointMemoryService_ProgressChanged(object sender, WatchDataReceiver.DataReceiverProgressEventArgs e)
        {
            SetProgressMessage(e.Text);
        }

        private async void SaveGPXFile(List<LogData> logDataEntries)
        {
            GpxClass gpx = new GpxClass();

            GpxClass.XmlWriterSettings.Encoding = System.Text.Encoding.UTF8;

            gpx.Metadata.time = SelectedLogHeader.HeaderTime;
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

            var headerTime = SelectedLogHeader.HeaderTime;
            var fileName = $"GPR-B1000-Route-{headerTime.Year}-{headerTime.Month}-{headerTime.Day}-2.gpx";

            var gpxString = gpx.ToXml();
            sharedPreferencesService.SetValue(Constants.PrefKeyGPX, gpxString);
            saveGPXFileService.SaveGPXFile(fileName);
        }

        private void SetProgressMessage(string message)
        {
            ProgressMessage = message;
        }

        #endregion

        #region Licensing callbacks
        private void HandleLicenseResponse(ILicenseDistributor licenseDistributor)
        {
            if (licenseDistributor.Validity == LicenseValidity.Invalid)
            {
                hasValidLicense = false;
            }
            else
            {
                hasValidLicense = true;
            }
        }

        private void HandleLicenseErrorResponse(ILicenseDistributor licenseDistributor)
        {
            if (!string.IsNullOrEmpty(licenseDistributor.ErrorCode))
            {
                logger.LogDebug($"Handling license checking error on Download. Error code: {licenseDistributor.ErrorCode}");
                ProgressMessage = $"Error occured during getting the license. Error code: {licenseDistributor.ErrorCode}";
            }
        }
        #endregion


        #region Properties
        public ObservableCollection<LogHeaderViewModel> LogHeaderList { get; } = new ObservableCollection<LogHeaderViewModel>();
        public LogHeaderViewModel SelectedLogHeader { get; set; }
        public string ProgressMessage { get => progressMessage; set { progressMessage = value; OnPropertyChanged("ProgressMessage"); } }
        
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

        #region Button commands
        public ICommand DisconnectCommand
        {
            get
            {
                if (disconnectCommand == null)
                {
                    disconnectCommand = new Command((o) => DisconnectButton_Clicked(), (o) => disconnectButtonCanBePressed);
                }

                return disconnectCommand;
            }
        }

        public ICommand DownloadHeadersCommand
        {
            get
            {
                if (downloadHeadersCommand == null)
                {
                    downloadHeadersCommand = new Command((o) => DownloadHeaders_Clicked(), (o) => downloadHeadersButtonCanBePressed);
                }

                return downloadHeadersCommand;
            }
        }

        public ICommand SaveGPXCommand
        {
            get
            {
                if (saveGPXCommand == null)
                {
                    saveGPXCommand = new Command((o) => DownloadSaveGPXButton_Clicked(), (o) => saveGPXButtonCanBePressed);
                }

                return saveGPXCommand;
            }
        }
        #endregion

        #endregion
    }
}
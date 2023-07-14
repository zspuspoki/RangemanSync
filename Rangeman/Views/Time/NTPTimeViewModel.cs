using Microsoft.Extensions.Logging;
using Rangeman.Services.BackgroundTimeSyncService;
using Rangeman.Services.BluetoothConnector;
using Rangeman.Services.NTP;
using Rangeman.Services.SharedPreferences;
using Rangeman.Services.WatchDataSender;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Rangeman.Views.Time
{
    public class NTPTimeViewModel : INotifyPropertyChanged
    {
        private NTPTimeInfo ntpTimeInfo;

        private bool watchCommandButtonsAreVisible = true;
        private bool disconnectButtonIsVisible = false;
        private bool startServiceButtonIsEnabled = true;
        private bool stopServiceButtonIsEnabled = false;

        private readonly BluetoothConnectorService bluetoothConnectorService;
        private readonly ILoggerFactory loggerFactory;
        private readonly ITimeSyncServiceStarter timeSyncServiceStarter;
        private readonly ITimeSyncServiceStatus timeSyncServiceStatus;
        private readonly ISharedPreferencesService sharedPreferencesService;
        private ILogger<NTPTimeViewModel> logger;

        public NTPTimeViewModel(BluetoothConnectorService bluetoothConnectorService,
            ILoggerFactory loggerFactory, ITimeSyncServiceStarter timeSyncServiceStarter, 
            ITimeSyncServiceStatus timeSyncServiceStatus, ISharedPreferencesService sharedPreferencesService)
        {
            this.logger = loggerFactory.CreateLogger<NTPTimeViewModel>();

            logger.LogInformation("Inside NTPTimeViewModel ctor");

            this.ntpTimeInfo = new NTPTimeInfo
            {
                NTPServer = sharedPreferencesService.GetValue(Constants.NTPServer, null),
                SecondsCompensation = int.Parse(sharedPreferencesService.GetValue(Constants.SecondsCompensation, "5")) 
            };

            this.CommitCommand = new Command<object>(this.OnCommit);
            this.DisconnectCommand = new Command(this.OnDisconnect);
            this.StartServiceCommad = new Command<object>(this.OnStartService);
            this.StopServiceCommand = new Command(this.OnStopService);

            this.bluetoothConnectorService = bluetoothConnectorService;
            this.loggerFactory = loggerFactory;
            this.timeSyncServiceStarter = timeSyncServiceStarter;
            this.timeSyncServiceStatus = timeSyncServiceStatus;
            this.sharedPreferencesService = sharedPreferencesService;

            MessagingCenter.Subscribe<ITimeSyncServiceStatus>(this, TimeSyncServiceMessages.ServiceStateChanged.ToString(),
                HandleTimeSyncServiceStateChange);
        }

        public void RefreshServiceButtonStates()
        {
            StopServiceButtonIsEnabled = timeSyncServiceStatus.GetState() == TimeSyncServiceState.Started;
            StartServiceButtonIsEnabled = !StopServiceButtonIsEnabled;
        }

        private void HandleTimeSyncServiceStateChange(ITimeSyncServiceStatus status)
        {
            var currentState = status.GetState();

            switch(currentState)
            {
                case TimeSyncServiceState.Closing:
                case TimeSyncServiceState.Starting:
                    StartServiceButtonIsEnabled = false;
                    StopServiceButtonIsEnabled = false;
                    break;

                case TimeSyncServiceState.Started:
                    StopServiceButtonIsEnabled = true;
                    StartServiceButtonIsEnabled = false;
                    break;

                case TimeSyncServiceState.Closed:
                    StopServiceButtonIsEnabled = false;
                    StartServiceButtonIsEnabled = true;
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public NTPTimeInfo NTPTimeInfo
        {
            get => ntpTimeInfo;
            set
            {
                this.ntpTimeInfo = value;
            }
        }

        public bool WatchCommandButtonsAreVisible
        {
            get => watchCommandButtonsAreVisible;
            set
            {
                watchCommandButtonsAreVisible = value;
                OnPropertyChanged("WatchCommandButtonsAreVisible");
            }
        }

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

        public bool StartServiceButtonIsEnabled
        {
            get => startServiceButtonIsEnabled;
            set
            {
                startServiceButtonIsEnabled = value;
                OnPropertyChanged(nameof(StartServiceButtonIsEnabled));
            }
        }

        public bool StopServiceButtonIsEnabled
        {
            get => stopServiceButtonIsEnabled;
            set
            {
                stopServiceButtonIsEnabled = value;
                OnPropertyChanged(nameof(StopServiceButtonIsEnabled));
            }
        }

        /// <summary>
        /// Gets or sets an ICommand implementation wrapping a commit action.
        /// </summary>
        public Command<object> CommitCommand { get; set; }

        public Command DisconnectCommand { get; set; }

        public Command<object> StartServiceCommad { get; set; }

        public Command StopServiceCommand { get; set; }

        private async void OnCommit(object dataForm)
        {
            bool isValid = ValidateForm(dataForm);
            if (!isValid)
            {
                NTPTimeInfo.ProgressMessage = "Please enter valid time details.";
                return;
            }

            SaveUserSetValues();

            await SendTimeToTheWatch();
        }

        private void SaveUserSetValues()
        {
            sharedPreferencesService.SetValue(Constants.NTPServer, NTPTimeInfo.NTPServer);
            sharedPreferencesService.SetValue(Constants.SecondsCompensation, NTPTimeInfo.SecondsCompensation.ToString());
        }

        private static bool ValidateForm(object dataForm)
        {
            var dataFormLayout = dataForm as Syncfusion.XForms.DataForm.SfDataForm;
            var isValid = dataFormLayout.Validate();
            dataFormLayout.Commit();
            return isValid;
        }

        private async void OnDisconnect()
        {
            logger.LogDebug("NTP time: Running OnDisconnect()");

            await bluetoothConnectorService.DisconnectFromWatch((msg) => NTPTimeInfo.ProgressMessage = msg);
            DisconnectButtonIsVisible = false;

            NTPTimeInfo.ProgressMessage = "Cancel button: The diconnection was successful.";
        }

        private async void OnStartService(object dataForm)
        {
            bool isValid = ValidateForm(dataForm);
            if (!isValid)
            {
                NTPTimeInfo.ProgressMessage = "Please enter valid time details.";
                return;
            }

            SaveUserSetValues();

            timeSyncServiceStarter.Start(ntpTimeInfo.NTPServer, ntpTimeInfo.SecondsCompensation.Value);
        }

        private async void OnStopService()
        {
            timeSyncServiceStarter.Stop();
        }

        private async Task SendTimeToTheWatch()
        {
            NTPTimeInfo.ProgressMessage = "Looking for Casio GPR-B1000 device. Please connect your watch.";
            await bluetoothConnectorService.FindAndConnectToWatch((message) => NTPTimeInfo.ProgressMessage = message,
                async (connection) =>
                {
                    logger.LogDebug("Custom Time tab - Device Connection was successful");
                    NTPTimeInfo.ProgressMessage = "Connected to GPR-B1000 watch.";

                    var watchDataSettingSenderService = new WatchDataSettingSenderService(connection, loggerFactory);

                    NTPTimeInfo.ProgressMessage = "Downloading exact time from the NTP server ...";

                    DateTime? currentTime = null;

                    currentTime = await GetNtpServerTime(currentTime);

                    if (currentTime != null)
                    {
                        logger.LogDebug("SendTimeToTheWatch() - currentTime is not null");

                        if (NTPTimeInfo.SecondsCompensation.HasValue)
                        {
                            logger.LogDebug("Seconds compensation has a value, adding it to the time value.");

                            currentTime = currentTime.Value.AddSeconds(NTPTimeInfo.SecondsCompensation.Value);
                        }

                        await SendCommandsToTheWatch(watchDataSettingSenderService, currentTime);
                    }

                    logger.LogDebug("Custom Time tab - after awaiting SendTime()");

                    DisconnectButtonIsVisible = false;

                    return true;
                },
                async () =>
                {
                    NTPTimeInfo.ProgressMessage = "An error occured during sending watch commands. Please try to connect again";
                    return true;
                },
                () => DisconnectButtonIsVisible = true);
        }

        private async Task SendCommandsToTheWatch(WatchDataSettingSenderService watchDataSettingSenderService, DateTime? currentTime)
        {
            try
            {
                Location location = null;

                try
                {
                    location = await Geolocation.GetLastKnownLocationAsync();
                }
                catch (Exception ex)
                {
                    logger.LogDebug("Current location is unknown");
                }

                await watchDataSettingSenderService.SendTime((ushort)currentTime.Value.Year, (byte)currentTime.Value.Month, (byte)currentTime.Value.Day,
                    (byte)currentTime.Value.Hour, (byte)currentTime.Value.Minute, (byte)currentTime.Value.Second,
                    (byte)currentTime.Value.DayOfWeek, 0, location?.Latitude, location?.Longitude);

                NTPTimeInfo.ProgressMessage = $"Finished sending current time ( {currentTime} ) to the watch.";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "NTP - An unexpected error occured during sending the watch commands to the watch");
                NTPTimeInfo.ProgressMessage = "An unexpected error occured during sending the time to the watch.";
            }
        }

        private async Task<DateTime?> GetNtpServerTime(DateTime? currentTime)
        {
            try
            {
                currentTime = await NtpClient.GetNetworkTimeAsync(ntpTimeInfo.NTPServer);

                NTPTimeInfo.ProgressMessage = "Sending current time to the watch ...";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occured during getting the time from the NTP server");
                NTPTimeInfo.ProgressMessage = "An unexpected error occured during getting the NTP server time";
            }

            return currentTime;
        }

        /// <summary>
        /// Occurs when propery value is changed.
        /// </summary>
        /// <param name="propName">Property name</param>
        private void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
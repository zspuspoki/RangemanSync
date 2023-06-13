using Microsoft.Extensions.Logging;
using Rangeman.Services.BluetoothConnector;
using Rangeman.Services.NTP;
using Rangeman.Services.WatchDataSender;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Rangeman.Views.Time
{
    public class NTPTimeViewModel : INotifyPropertyChanged
    {
        private NTPTimeInfo ntpTimeInfo;

        private bool watchCommandButtonsAreVisible = true;
        private bool disconnectButtonIsVisible = false;
        private readonly BluetoothConnectorService bluetoothConnectorService;
        private readonly ILoggerFactory loggerFactory;
        private ILogger<NTPTimeViewModel> logger;

        public NTPTimeViewModel(BluetoothConnectorService bluetoothConnectorService,
            ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<NTPTimeViewModel>();

            logger.LogInformation("Inside NTPTimeViewModel ctor");

            this.ntpTimeInfo = new NTPTimeInfo { SecondsCompensation = 5 };

            this.CommitCommand = new Command<object>(this.OnCommit);
            this.DisconnectCommand = new Command(this.OnDisconnect);

            this.bluetoothConnectorService = bluetoothConnectorService;
            this.loggerFactory = loggerFactory;
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

        /// <summary>
        /// Gets or sets an ICommand implementation wrapping a commit action.
        /// </summary>
        public Command<object> CommitCommand { get; set; }

        public Command DisconnectCommand { get; set; }

        private async void OnCommit(object dataForm)
        {
            var dataFormLayout = dataForm as Syncfusion.XForms.DataForm.SfDataForm;
            var isValid = dataFormLayout.Validate();
            dataFormLayout.Commit();
            if (!isValid)
            {
                NTPTimeInfo.ProgressMessage = "Please enter valid time details.";
                return;
            }

            await SendTimeToTheWatch();
        }

        private async void OnDisconnect()
        {
            logger.LogDebug("NTP time: Running OnDisconnect()");

            await bluetoothConnectorService.DisconnectFromWatch((msg) => NTPTimeInfo.ProgressMessage = msg);
            DisconnectButtonIsVisible = false;

            NTPTimeInfo.ProgressMessage = "Cancel button: The diconnection was successful.";
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
                await watchDataSettingSenderService.SendTime((ushort)currentTime.Value.Year, (byte)currentTime.Value.Month, (byte)currentTime.Value.Day,
                    (byte)currentTime.Value.Hour, (byte)currentTime.Value.Minute, (byte)currentTime.Value.Second,
                    (byte)currentTime.Value.DayOfWeek, 0);

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
using Microsoft.Extensions.Logging;
using Rangeman.Services.BluetoothConnector;
using Rangeman.Services.WatchDataSender;
using Rangeman.Views.Common;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Rangeman.Views.Time
{
    public class CustomTimeViewModel : INotifyPropertyChanged
    {
        private CustomTimeInfo customTimeInfo;
        private ILogger<CustomTimeViewModel> logger;
        private bool watchCommandButtonsAreVisible = true;
        private bool disconnectButtonIsVisible = false;
        private readonly BluetoothConnectorService bluetoothConnectorService;
        private readonly ILoggerFactory loggerFactory;

        public CustomTimeViewModel(BluetoothConnectorService bluetoothConnectorService,
            ILoggerFactory loggerFactory, ITimeInfoValidator timeInfoValidator)
        {
            this.logger = loggerFactory.CreateLogger<CustomTimeViewModel>();

            logger.LogInformation("Inside CustomTimeViewModel ctor");

            var currentDate = DateTime.Now;
            this.customTimeInfo = new CustomTimeInfo(timeInfoValidator)
            {
                Year = currentDate.Year,
                Month = (MonthType)currentDate.Month,
                Day = currentDate.Day,
                Hour = currentDate.Hour,
                Minute = currentDate.Minute,
                Second = currentDate.Second,
                DayOfWeek = Enum.Parse<DayOfWeekType>(currentDate.DayOfWeek.ToString())
            };
            
            this.CommitCommand = new Command<object>(this.OnCommit);
            this.DisconnectCommand = new Command(this.OnDisconnect);

            this.bluetoothConnectorService = bluetoothConnectorService;
            this.loggerFactory = loggerFactory;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CustomTimeInfo CustomTimeInfo
        {
            get => customTimeInfo;
            set
            {
                this.customTimeInfo = value;
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
                CustomTimeInfo.ProgressMessage = "Please enter valid time details.";
                return;
            }

            await SendTimeToTheWatch();
        }

        private async void OnDisconnect()
        {
            logger.LogDebug("Custom time: Running OnDisconnect()");

            await bluetoothConnectorService.DisconnectFromWatch((msg) => CustomTimeInfo.ProgressMessage = msg);
            DisconnectButtonIsVisible = false;

            CustomTimeInfo.ProgressMessage = "Cancel button: The diconnection was successful.";
        }

        private async Task SendTimeToTheWatch()
        {
            CustomTimeInfo.ProgressMessage = "Looking for Casio GPR-B1000 device. Please connect your watch.";
            await bluetoothConnectorService.FindAndConnectToWatch((message) => CustomTimeInfo.ProgressMessage = message,
                async (connection) =>
                {
                    try
                    {
                        logger.LogDebug("Custom Time tab - Device Connection was successful");
                        CustomTimeInfo.ProgressMessage = "Connected to GPR-B1000 watch.";

                        var watchDataSettingSenderService = new WatchDataSettingSenderService(connection, loggerFactory);

                        CustomTimeInfo.ProgressMessage = "Sending custom time to the watch ...";

                        await watchDataSettingSenderService.SendTime((ushort)customTimeInfo.Year.Value, (byte)customTimeInfo.Month, (byte)customTimeInfo.Day.Value,
                            (byte)customTimeInfo.Hour.Value, (byte)customTimeInfo.Minute.Value, (byte)customTimeInfo.Second.Value,
                            (byte)customTimeInfo.DayOfWeek, 0);

                        CustomTimeInfo.ProgressMessage = "Finished sending time to the watch.";

                        logger.LogDebug("Custom Time tab - after awaiting SendTime()");

                        DisconnectButtonIsVisible = false;

                        return true;
                    }
                    catch(Exception ex)
                    {
                        CustomTimeInfo.ProgressMessage = "An unexpected error occured during sending the time to the watch.";
                        logger.LogError(ex, "An unexpected error occured during sending the custom set time to the watch");
                        return false;
                    }
                },
                async () =>
                {
                    CustomTimeInfo.ProgressMessage = "An error occured during sending watch commands. Please try to connect again";
                    return true;
                },
                () => DisconnectButtonIsVisible = true);
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
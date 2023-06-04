using Microsoft.Extensions.Logging;
using Rangeman.Services.BluetoothConnector;
using Rangeman.Services.WatchDataSender;
using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace Rangeman.Views.Time
{
    public class CustomTimeViewModel : INotifyPropertyChanged
    {
        private CustomTimeInfo customTimeInfo;
        private ILogger<CustomTimeViewModel> logger;
        private readonly BluetoothConnectorService bluetoothConnectorService;
        private readonly ILoggerFactory loggerFactory;

        public CustomTimeViewModel(BluetoothConnectorService bluetoothConnectorService,
            ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<CustomTimeViewModel>();

            logger.LogInformation("Inside CustomTimeViewModel ctor");

            var currentDate = DateTime.Now;
            this.customTimeInfo = new CustomTimeInfo
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

        /// <summary>
        /// Gets or sets an ICommand implementation wrapping a commit action.
        /// </summary>
        public Command<object> CommitCommand { get; set; }

        private async void OnCommit(object dataForm)
        {
            CustomTimeInfo.ProgressMessage = "Looking for Casio GPR-B1000 device. Please connect your watch.";
            await bluetoothConnectorService.FindAndConnectToWatch((message) => CustomTimeInfo.ProgressMessage = message,
                async (connection) =>
                {
                    logger.LogDebug("Custom Time tab - Device Connection was successful");
                    CustomTimeInfo.ProgressMessage = "Connected to GPR-B1000 watch.";

                    var watchDataSettingSenderService = new WatchDataSettingSenderService(connection, loggerFactory);
                    await watchDataSettingSenderService.SendTime((ushort)customTimeInfo.Year.Value, (byte)customTimeInfo.Month, (byte)customTimeInfo.Day.Value, 
                        (byte)customTimeInfo.Hour.Value, (byte)customTimeInfo.Minute.Value, (byte)customTimeInfo.Second.Value,
                        (byte)customTimeInfo.DayOfWeek, 0);

                    logger.LogDebug("Custom Time tab - after awaiting SendRoute()");

                    return true;
                },
                async () =>
                {
                    CustomTimeInfo.ProgressMessage = "An error occured during sending watch commands. Please try to connect again";
                    return true;
                });
        }
    }
}
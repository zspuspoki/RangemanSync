using Microsoft.Extensions.Logging;
using Rangeman.Services.BluetoothConnector;
using Rangeman.Services.NTP;
using Rangeman.Services.WatchDataSender;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Rangeman.Views.Time
{
    public class NTPTimeViewModel
    {
        private NTPTimeInfo ntpTimeInfo;
        private readonly BluetoothConnectorService bluetoothConnectorService;
        private readonly ILoggerFactory loggerFactory;
        private ILogger<NTPTimeViewModel> logger;

        public NTPTimeViewModel(BluetoothConnectorService bluetoothConnectorService,
            ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<NTPTimeViewModel>();

            logger.LogInformation("Inside NTPTimeViewModel ctor");

            this.ntpTimeInfo = new NTPTimeInfo();
            this.CommitCommand = new Command<object>(this.OnCommit);
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

        /// <summary>
        /// Gets or sets an ICommand implementation wrapping a commit action.
        /// </summary>
        public Command<object> CommitCommand { get; set; }

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

                    var currentTime = await NtpClient.GetNetworkTimeAsync(ntpTimeInfo.NTPServer);

                    NTPTimeInfo.ProgressMessage = "Sending current time to the watch ...";

                    await watchDataSettingSenderService.SendTime((ushort)currentTime.Year, (byte)currentTime.Month, (byte)currentTime.Day,
                        (byte)currentTime.Hour, (byte)currentTime.Minute, (byte)currentTime.Second,
                        (byte)currentTime.DayOfWeek, 0);

                    NTPTimeInfo.ProgressMessage = $"Finished sending current time ( {currentTime} ) to the watch.";

                    logger.LogDebug("Custom Time tab - after awaiting SendTime()");

                    return true;
                },
                async () =>
                {
                    NTPTimeInfo.ProgressMessage = "An error occured during sending watch commands. Please try to connect again";
                    return true;
                });
        }
    }
}
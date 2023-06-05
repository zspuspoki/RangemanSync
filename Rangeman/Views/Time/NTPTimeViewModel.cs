using Microsoft.Extensions.Logging;
using Rangeman.Services.BluetoothConnector;
using System.ComponentModel;
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

        private void OnCommit(object dataForm)
        {
            var dataFormLayout = dataForm as Syncfusion.XForms.DataForm.SfDataForm;
            var isValid = dataFormLayout.Validate();
            dataFormLayout.Commit();
            if (!isValid)
            {
                NTPTimeInfo.ProgressMessage = "Please enter valid time details.";
                return;
            }

        }
    }
}
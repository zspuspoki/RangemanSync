using Android.Service.VR;
using Microsoft.Extensions.Logging;
using Rangeman.Services.BluetoothConnector;
using Rangeman.Services.WatchDataSender;
using Rangeman.Views.Common;
using Rangeman.Views.Time;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Rangeman.Views.Tide
{
    public class TideViewModel : BaseDomainInfo
    {
        private TideInfo tideInfo;
        private ILogger<TideViewModel> logger;
        private bool watchCommandButtonsAreVisible = true;
        private bool disconnectButtonIsVisible = false;
        private readonly BluetoothConnectorService bluetoothConnectorService;
        private readonly ILoggerFactory loggerFactory;

        public TideViewModel(BluetoothConnectorService bluetoothConnectorService,
            ILoggerFactory loggerFactory, ITimeInfoValidator timeInfoValidator)
        {
            this.logger = loggerFactory.CreateLogger<TideViewModel>();

            logger.LogInformation("Inside CustomTimeViewModel ctor");

            var currentDate = DateTime.Now;
            this.TideInfo = new TideInfo(timeInfoValidator)
            {
                Year = currentDate.Year,
                Month = (MonthType)currentDate.Month,
                Day = currentDate.Day,
                Hour = currentDate.Hour,
                Minute = currentDate.Minute,
                CityName = "DEFAULTCITY",
                GPSCoordinates = "45.68333333, 13.38333333"
            };
        }

        public TideInfo TideInfo
        {
            get => tideInfo;
            set
            {
                this.tideInfo = value;
            }
        }

        public bool WatchCommandButtonsAreVisible
        {
            get => watchCommandButtonsAreVisible;
            set
            {
                watchCommandButtonsAreVisible = value;
                RaisePropertyChanged("WatchCommandButtonsAreVisible");
            }
        }

        public bool DisconnectButtonIsVisible
        {
            get => disconnectButtonIsVisible;
            set
            {
                disconnectButtonIsVisible = value;
                RaisePropertyChanged("DisconnectButtonIsVisible");
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
                TideInfo.ProgressMessage = "Please enter valid time details.";
                return;
            }

            await SendTideToTheWatch();
        }

        private async Task SendTideToTheWatch()
        {
            TideInfo.ProgressMessage = "Looking for Casio GPR-B1000 device. Please connect your watch.";
            await bluetoothConnectorService.FindAndConnectToWatch((message) => TideInfo.ProgressMessage = message,
                async (connection) =>
                {
                    try
                    {
                        logger.LogDebug("Tide tab - Device Connection was successful");
                        TideInfo.ProgressMessage = "Connected to GPR-B1000 watch.";

                        var watchDataSettingSenderService = new WatchDataSettingSenderService(connection, loggerFactory);

                        TideInfo.ProgressMessage = "Sending custom tide to the watch ...";

                        if(!string.IsNullOrWhiteSpace(this.tideInfo.GPSCoordinates))
                        {
                            var splittedGPSCoords = this.tideInfo.GPSCoordinates.Split(",");

                            if (splittedGPSCoords.Length == 2)
                            {
                                if (double.TryParse(splittedGPSCoords[0], out var latitude) && 
                                double.TryParse(splittedGPSCoords[1], out var longitude))
                                {
                                    await watchDataSettingSenderService.SendTide(this.tideInfo.CityName, latitude, longitude,
                                        (ushort)this.tideInfo.Year, (byte)tideInfo.Month, (byte)tideInfo.Day.Value,
                                        (byte)tideInfo.Hour.Value, (byte)tideInfo.Minute.Value);

                                    TideInfo.ProgressMessage = "Finished sending tide to the watch.";
                                }
                            }
                            else
                            {
                                TideInfo.ProgressMessage = "An unexpected GPS coordinate splitting error occured";
                            }
                        }
                        else
                        {
                            TideInfo.ProgressMessage = "An unexpected GPS coordinates interpretation error occured";
                        }


                        TideInfo.ProgressMessage = "Finished sending tide to the watch.";

                        logger.LogDebug("Tide tab - after awaiting SendTime()");

                        DisconnectButtonIsVisible = false;

                        return true;
                    }
                    catch (Exception ex)
                    {
                        TideInfo.ProgressMessage = "An unexpected error occured during sending the tide info to the watch.";
                        logger.LogError(ex, "An unexpected error occured during sending the custom tide to the watch");
                        return false;
                    }
                },
                async () =>
                {
                    TideInfo.ProgressMessage = "An error occured during sending watch commands. Please try to connect again";
                    return true;
                },
                () => DisconnectButtonIsVisible = true);
        }

    }
}
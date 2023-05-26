using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rangeman.Services.BluetoothConnector;
using Rangeman.Services.SharedPreferences;
using Rangeman.Views.Map;
using Rangeman.WatchDataSender;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace Rangeman.Views.Coordinates
{
    public class CoordinatesViewModel : ViewModelBase, INodesViewModel
    {
        private ObservableCollection<CoordinateInfo> coordinateInfo;
        public ObservableCollection<CoordinateInfo> CoordinateInfoCollection
        {
            get { return coordinateInfo; }
            set { this.coordinateInfo = value; }
        }

        private string progressMessage = "Please fill in the below table and tap 'send to the watch' once you are ready.";
        public string ProgressMessage 
        { 
            get => progressMessage; 
            set 
            { 
                progressMessage = value; 
                OnPropertyChanged("ProgressMessage"); 
            } 
        }

        private readonly BluetoothConnectorService bluetoothConnectorService;
        private readonly ILoggerFactory loggerFactory;
        private readonly ISaveCoordinatesDataService saveCoordinatesDataService;
        private readonly ISharedPreferencesService sharedPreferencesService;
        private ILogger<CoordinatesViewModel> logger;
        private readonly CoordinateInfoValidator coordinateInfoValidator;

        #region Commands
        private bool sendToWatchButtonCanBePressed = true;
        private ICommand sendToWatchCommand;

        public ICommand SendToWatchCommand
        {
            get
            {
                if (sendToWatchCommand == null)
                {
                    sendToWatchCommand = new Command((o) => SendToWatch_Clicked(), (o) => sendToWatchButtonCanBePressed);
                }

                return sendToWatchCommand;
            }
        }

        private bool disconnectButtonCanBePressed = true;
        private ICommand disconnectCommand;

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

        private bool saveButtonCanBePressed = true;
        private ICommand saveCommand;

        public ICommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                {
                    saveCommand = new Command((o) => SaveButton_Clicked(), (o) => saveButtonCanBePressed);
                }

                return saveCommand;
            }
        }
        #endregion

        private bool watchCommandButtonsAreVisible = true;
        public bool WatchCommandButtonsAreVisible { get => watchCommandButtonsAreVisible; set { watchCommandButtonsAreVisible = value; OnPropertyChanged("WatchCommandButtonsAreVisible"); } }

        private bool disconnectButtonIsVisible = false;
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

        public CoordinatesViewModel(BluetoothConnectorService bluetoothConnectorService, ILoggerFactory loggerFactory, 
            ISaveCoordinatesDataService saveCoordinatesDataService,ISharedPreferencesService sharedPreferencesService)
        {
            this.logger = loggerFactory.CreateLogger<CoordinatesViewModel>();
            coordinateInfo = new ObservableCollection<CoordinateInfo>();
            this.GenerateCoordinateInfo();
            this.bluetoothConnectorService = bluetoothConnectorService;
            this.loggerFactory = loggerFactory;
            this.saveCoordinatesDataService = saveCoordinatesDataService;
            this.sharedPreferencesService = sharedPreferencesService;
            this.coordinateInfoValidator = new CoordinateInfoValidator(coordinateInfo);
        }

        #region Button click handler methods
        private async void SendToWatch_Clicked()
        {
            if(!coordinateInfoValidator.IsValid(out var erorMessages))
            {
                ProgressMessage = $"The entered data in the table is invalid. Errors: {erorMessages}";
                return;
            }

            ProgressMessage = "Looking for Casio GPR-B1000 device. Please connect your watch.";

            await bluetoothConnectorService.FindAndConnectToWatch((message) => ProgressMessage = message,
                async (connection) =>
                {
                    logger.LogDebug("Coordinates tab - Device Connection was successful");
                    ProgressMessage = "Connected to GPR-B1000 watch.";

                    MapPageDataConverter mapPageDataConverter = new MapPageDataConverter(this, loggerFactory);

                    var watchDataSenderService = new WatchDataSenderService(connection, mapPageDataConverter.GetDataByteArray(),
                        mapPageDataConverter.GetHeaderByteArray(), loggerFactory);

                    watchDataSenderService.ProgressChanged += WatchDataSenderService_ProgressChanged;
                    await watchDataSenderService.SendRoute();

                    logger.LogDebug("Map tab - after awaiting SendRoute()");

                    DisconnectButtonIsVisible = false;
                    return true;
                },
                async () =>
                {
                    ProgressMessage = "An error occured during sending watch commands. Please try to connect again";
                    return true;
                },
                ()=> DisconnectButtonIsVisible = true);
        }

        private async void DisconnectButton_Clicked()
        {
            disconnectButtonCanBePressed = false;
            await bluetoothConnectorService.DisconnectFromWatch(SetProgressMessage);
            DisconnectButtonIsVisible = false;
            disconnectButtonCanBePressed = true;
        }

        private async void SaveButton_Clicked()
        {
            saveButtonCanBePressed = false;
            var tableDataToSave = JsonConvert.SerializeObject(coordinateInfo);
            sharedPreferencesService.SetValue(Constants.PrefSaveCoordinatesData, tableDataToSave);
            var guid = Guid.NewGuid();
            saveCoordinatesDataService.SaveCoordinatesData($"Coordinates_{guid}.xml");
            saveButtonCanBePressed = true;
        }
        #endregion

        private void WatchDataSenderService_ProgressChanged(object sender, DataSenderProgressEventArgs e)
        {
            ProgressMessage = $"Status: {e.Text}, Progress: {e.PercentageNumber}";

            logger.LogDebug($"Current progress bar percentage number: {e.PercentageNumber}");
        }

        private void SetProgressMessage(string message)
        {
            ProgressMessage = message;
        }


        private void GenerateCoordinateInfo()
        {
            coordinateInfo.Add(new CoordinateInfo { NodeName = "S", Coordinates = "", CoordinateName="" });
            coordinateInfo.Add(new CoordinateInfo { NodeName = "1", Coordinates = "", CoordinateName = "" });
            coordinateInfo.Add(new CoordinateInfo { NodeName = "2", Coordinates = "", CoordinateName = "" });
            coordinateInfo.Add(new CoordinateInfo { NodeName = "3", Coordinates = "", CoordinateName = "" });
            coordinateInfo.Add(new CoordinateInfo { NodeName = "4", Coordinates = "", CoordinateName = "" });
            coordinateInfo.Add(new CoordinateInfo { NodeName = "5", Coordinates = "", CoordinateName = "" });
            coordinateInfo.Add(new CoordinateInfo { NodeName = "6", Coordinates = "", CoordinateName = "" });
            coordinateInfo.Add(new CoordinateInfo { NodeName = "7", Coordinates = "", CoordinateName = "" });
            coordinateInfo.Add(new CoordinateInfo { NodeName = "8", Coordinates = "", CoordinateName = "" });
            coordinateInfo.Add(new CoordinateInfo { NodeName = "9", Coordinates = "", CoordinateName = "" });
            coordinateInfo.Add(new CoordinateInfo { NodeName = "G", Coordinates = "", CoordinateName = "" });
        }

        private void GetCoordinates(List<GpsCoordinatesViewModel> result, Func<CoordinateInfo, bool> coordinateTypeIsAcceptableFunc)
        {
            foreach (var enteredCoordinate in coordinateInfo)
            {
                if (coordinateTypeIsAcceptableFunc(enteredCoordinate))
                {
                    if (!string.IsNullOrWhiteSpace(enteredCoordinate.Coordinates))
                    {
                        var splittedValue = enteredCoordinate.Coordinates.Split(',');
                        if (splittedValue.Length == 2)
                        {
                            if (double.TryParse(splittedValue[0], out double latitude) && double.TryParse(splittedValue[1], out double longitude))
                            {
                                result.Add(new GpsCoordinatesViewModel { Latitude = latitude, Longitude = longitude });
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<GpsCoordinatesViewModel> GetStartEndCoordinates(bool removeEmptyEntries = true)
        {
            var result = new List<GpsCoordinatesViewModel>();

            GetCoordinates(result, (c) => c.NodeName == "S" || c.NodeName == "G");

            return result;
        }

        public List<GpsCoordinatesViewModel> GetTransitPointCoordinates(bool removeEmptyEntries = true)
        {
            var result = new List<GpsCoordinatesViewModel>();

            GetCoordinates(result, (c) => c.NodeName != "S" && c.NodeName != "G");

            return result;
        }
    }
}
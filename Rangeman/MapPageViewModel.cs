using Android.Content;
using System.Collections.Generic;

namespace Rangeman
{
    internal class MapPageViewModel : ViewModelBase
    {
        private List<GpsCoordinates> gpsCoordinates = new List<GpsCoordinates>();
        private bool hasStartCoordinate;
        private bool hasEndCoordinate;
        private bool progressBarIsVisible;
        private string progressBarPercentageMessage;
        private string progressMessage;
        private double progressBarPercentageNumber;

        public MapPageViewModel(Android.Content.Context context)
        {
            Context = context;
        }

        public void AddStartCoordinates(double longitude, double latitude)
        {
            gpsCoordinates.Add(new GpsCoordinates { Longitude = longitude, Latitude = latitude });
            hasStartCoordinate = true;
        }

        public void AddEndCoordinates(double longitude, double latitude)
        {
            gpsCoordinates.Add(new GpsCoordinates { Longitude = longitude, Latitude = latitude });
            hasEndCoordinate = true;
            HasRoute = true;
        }

        public void ResetCoordinates()
        {
            gpsCoordinates.Clear();
            HasRoute = false;
        }

        public bool ProgressBarIsVisible { get => progressBarIsVisible; set { progressBarIsVisible = value; OnPropertyChanged("ProgressBarIsVisible"); } }
        public string ProgressBarPercentageMessage { get => progressBarPercentageMessage; set { progressBarPercentageMessage = value; OnPropertyChanged("ProgressBarPercentageMessage"); } }
        public double ProgressBarPercentageNumber { get => progressBarPercentageNumber; set { progressBarPercentageNumber = value; OnPropertyChanged("ProgressBarPercentageNumber"); } }
        public string ProgressMessage { get => progressMessage; set { progressMessage = value; OnPropertyChanged("ProgressMessage"); } }
        public Context Context { get; }
        public bool HasStartCoordinate => hasStartCoordinate;
        public bool HasEndCoordinate => hasEndCoordinate;
        public IEnumerable<GpsCoordinates> GpsCoordinates => gpsCoordinates;
        public bool HasRoute { get; set; }

    }
}
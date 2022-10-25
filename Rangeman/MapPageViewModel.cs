using Android.Content;
using System.Collections.Generic;

namespace Rangeman
{
    internal class MapPageViewModel : ViewModelBase
    {
        public const int MaxNumberOfTransitPoints = 9;

        private List<GpsCoordinates> startEndCoordinates = new List<GpsCoordinates>();
        private List<GpsCoordinates> transitPointCordinates = new List<GpsCoordinates>();

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
            startEndCoordinates.Add(new GpsCoordinates { Longitude = longitude, Latitude = latitude });
            hasStartCoordinate = true;
        }

        public void AddEndCoordinates(double longitude, double latitude)
        {
            startEndCoordinates.Add(new GpsCoordinates { Longitude = longitude, Latitude = latitude });
            hasEndCoordinate = true;
            HasRoute = true;
        }

        public void AddTransitPointCoordinates(double longitude, double latitude)
        {
            transitPointCordinates.Add(new GpsCoordinates { Longitude = longitude, Latitude = latitude });
        }

        public void ResetCoordinates()
        {
            startEndCoordinates.Clear();
            HasRoute = false;
        }

        public bool ProgressBarIsVisible { get => progressBarIsVisible; set { progressBarIsVisible = value; OnPropertyChanged("ProgressBarIsVisible"); } }
        public string ProgressBarPercentageMessage { get => progressBarPercentageMessage; set { progressBarPercentageMessage = value; OnPropertyChanged("ProgressBarPercentageMessage"); } }
        public double ProgressBarPercentageNumber { get => progressBarPercentageNumber; set { progressBarPercentageNumber = value; OnPropertyChanged("ProgressBarPercentageNumber"); } }
        public string ProgressMessage { get => progressMessage; set { progressMessage = value; OnPropertyChanged("ProgressMessage"); } }
        public Context Context { get; }
        public bool HasStartCoordinate => hasStartCoordinate;
        public bool HasEndCoordinate => hasEndCoordinate;
        public IEnumerable<GpsCoordinates> StartEndCoordinates => startEndCoordinates;
        public IEnumerable<GpsCoordinates> TransitPointCoordinates => transitPointCordinates;
        public bool HasRoute { get; set; }

    }
}
using Android.Content;
using System.Collections.Generic;

namespace Rangeman
{
    internal class MapPageViewModel
    {
        private List<GpsCoordinates> gpsCoordinates = new List<GpsCoordinates>();
        private bool hasStartCoordinate;
        private bool hasEndCoordinate;

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
        }

        public bool ProgressBarIsVisible { get; set; }
        public string ProgressMessage { get; set; } = "Test message";
        public Context Context { get; }
        public bool HasStartCoordinate => hasStartCoordinate;
        public bool HasEndCoordinate => hasEndCoordinate;
        public IEnumerable<GpsCoordinates> GpsCoordinates => gpsCoordinates;

    }
}
namespace Rangeman
{
    public class GpsCoordinatesViewModel
    {
        private const double INVALIDLONGLAT = -1500;
        public double Longitude { get; set; } = INVALIDLONGLAT;
        public double Latitude { get; set; } = INVALIDLONGLAT;

        public bool HasValidCoordinates
        {
            get
            {
                return Latitude >= -90 && Latitude <= 90 &&
                    Longitude >= -180 && Longitude <= 180;
            }
        }
    }
}
namespace Rangeman
{
    public class GpsCoordinatesViewModel
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }

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
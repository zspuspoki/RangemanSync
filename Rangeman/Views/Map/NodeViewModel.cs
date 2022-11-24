using System;

namespace Rangeman.Views.Map
{
    public enum NodeCategory
    {
        StartEnd,
        Transit
    }
    public class NodeViewModel : IEquatable<NodeViewModel>
    {
        private const double INVALIDLONGLAT = -1500;
        public string Title { get; set; }
        public double Longitude { get; set; } = INVALIDLONGLAT;
        public double Latitude { get; set; } = INVALIDLONGLAT;
        public bool Visible { get; set; }
        public NodeCategory Category { get; set; }

        public bool HasValidCoordinates
        {
            get
            {
                return Latitude >= -90 && Latitude <= 90 &&
                    Longitude >= -180 && Longitude <= 180;
            }
        }

        public void InvalidateLongLatValues()
        {
            Longitude = INVALIDLONGLAT;
            Latitude = INVALIDLONGLAT;
        }

        public bool Equals(NodeViewModel other)
        {
            if(other.Title == Title)
            {
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return $"Set {Title}";
        }
    }
}
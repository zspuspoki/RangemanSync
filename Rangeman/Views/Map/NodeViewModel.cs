using System;

namespace Rangeman.Views.Map
{
    public enum NodeCategory
    {
        StartEnd,
        Transit
    }
    internal class NodeViewModel : IEquatable<NodeViewModel>
    {
        public string Title { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public bool Visible { get; set; }
        public NodeCategory Category { get; set; }

        public bool Equals(NodeViewModel other)
        {
            if(other.Title == Title)
            {
                return true;
            }

            return false;
        }
    }
}
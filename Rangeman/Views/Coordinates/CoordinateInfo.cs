using System.ComponentModel;

namespace Rangeman.Views.Coordinates
{
    public class CoordinateInfo : INotifyPropertyChanged
    {
        private string nodeName;
        private string coordinates;
        private string coordinateName;

        public string NodeName 
        { 
            get
            {
                return nodeName;
            }

            set
            {
                this.nodeName = value;
                RaisePropertyChanged("NodeName");
            }
        }

        public string Coordinates 
        { 
            get
            {
                return coordinates;
            }

            set
            {
                this.coordinates = value;
                RaisePropertyChanged("Coordinates");
            }
        }

        public string CoordinateName 
        { 
            get
            {
                return coordinateName;
            }

            set
            {
                this.coordinateName = value;
                RaisePropertyChanged("CoordinateName");
            }
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string Name)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(Name));
        }
        #endregion
    }
}
using Mapsui.UI.Forms;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Location = Xamarin.Essentials.Location;

namespace Rangeman.Views.Map
{
    public class AddressPanelViewModel : ViewModelBase
    {
        private string street;
        private string city;
        private string country;
        private ICommand placeOnMapCommand;
        private ICommand showOnMap;
        private Position position;
        private bool useGPSCoordinatesInsteadOfAddress;
        private bool canDisplayAddressEntries = true;
        private double longitude;
        private double latitude;
        private readonly IMapPageView mapPageView;

        public AddressPanelViewModel(IMapPageView mapPageView)
        {
            this.mapPageView = mapPageView;
        }

        public async void PlaceOnMap()
        {
            try
            {
                await SetPosition();
                mapPageView.PlaceOnMapClicked(position);
            }
            catch(Exception ex)
            {
                mapPageView.DisplayProgressMessage("An unexpected error occured during setting the address. Do you have internet connection?");
            }
        }

        public async void ShowOnMap()
        {
            try
            {
                await SetPosition();
                mapPageView.ShowOnMap(position);
            }
            catch(Exception ex)
            {
                mapPageView.DisplayProgressMessage("An unexpected error occured during setting the address. Do you have internet connection?");
            }
        }

        public async Task UpdateUserPositionAsync()
        {
            var location = await Geolocation.GetLastKnownLocationAsync();
            position = new Position(location.Latitude, location.Longitude);
            
            Longitude = position.Longitude;
            Latitude = position.Latitude;
            
            await SetAddressAsync(position);
        }

        private async Task SetPosition()
        {
            double latitude = this.latitude;
            double longitude = this.longitude;

            if (!UseGPSCoordinatesInsteadOfAddress)
            {
                var location = (await Geocoding.GetLocationsAsync($"{street}, {city}, {country}")).FirstOrDefault();

                if (location == null) return;

                latitude = location.Latitude;
                longitude = location.Longitude;
            }

            if (!IsLongitudeValid || !IsLatitudeValid)
            {
                return;
            }

            position = new Position(latitude, longitude);
        }

        private async Task SetAddressAsync(Position p)
        {
            var addrs = (await Geocoding.GetPlacemarksAsync(new Location(p.Latitude, p.Longitude))).FirstOrDefault();
            Street = $"{addrs.Thoroughfare} {addrs.SubThoroughfare}";
            City = $"{addrs.PostalCode} {addrs.Locality}";
            Country = addrs.CountryName;
        }

        public string Street { get => street; set { street = value; OnPropertyChanged("Street"); } }
        public string City { get => city; set { city = value; OnPropertyChanged("City"); } }
        public string Country { get => country; set { country = value; OnPropertyChanged("Country"); } }

        public ICommand PlaceOnMapCommand
        {
            get
            {
                if (placeOnMapCommand == null)
                {
                    placeOnMapCommand = new Command((o) => PlaceOnMap(), (o) => true);
                }

                return placeOnMapCommand;
            }
        }

        public ICommand ShowOnMapCommand
        {
            get
            {
                if (showOnMap == null)
                {
                    showOnMap = new Command((o) => ShowOnMap(), (o) => true);
                }

                return showOnMap;
            }
        }

        public bool UseGPSCoordinatesInsteadOfAddress 
        { 
            get=> useGPSCoordinatesInsteadOfAddress; 
            set 
            { 
                useGPSCoordinatesInsteadOfAddress = value; 
                OnPropertyChanged("UseGPSCoordinatesInsteadOfAddress");
                CanDisplayAddressEntries = !useGPSCoordinatesInsteadOfAddress;
            } 
        }

        public bool CanDisplayAddressEntries 
        { 
            get => canDisplayAddressEntries; 
            set 
            { 
                canDisplayAddressEntries = value; 
                OnPropertyChanged("CanDisplayAddressEntries"); 
            } 
        }

        public double Longitude 
        { 
            get => longitude; 
            set 
            { 
                longitude = value; 
                OnPropertyChanged("Longitude"); 
            } 
        }

        public double Latitude 
        { 
            get => latitude; 
            set 
            { 
                latitude = value; 
                OnPropertyChanged("Latitude"); 
            } 
        }

        public bool IsLongitudeValid { get; set; }
        public bool IsLatitudeValid { get; set; }
    }
}
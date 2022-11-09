using Mapsui.UI.Forms;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Rangeman.Views.Map
{
    internal class AddressPanelViewModel : ViewModelBase
    {
        private string street;
        private string city;
        private string country;
        private ICommand placeOnMapCommand;
        private Position position;

        public event EventHandler<Position> PlaceOnMapClicked;

        public async void PlaceOnMap()
        {
            var location = (await Geocoding.GetLocationsAsync($"{street}, {city}, {country}")).FirstOrDefault();

            if (location == null) return;

            position = new Position(location.Latitude, location.Longitude);

            if(PlaceOnMapClicked != null)
            {
                PlaceOnMapClicked(this, position);
            }
        }

        public async Task UpdateUserPositionAsync()
        {
            var location = await Geolocation.GetLastKnownLocationAsync();
            position = new Position(location.Latitude, location.Longitude);
            
            await SetAddressAsync(position);
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
    }
}
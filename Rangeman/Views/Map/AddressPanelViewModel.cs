using Mapsui.UI.Forms;
using Rangeman.Services.PhoneLocation;
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
        private string longitude;
        private string latitude;
        private readonly IMapPageView mapPageView;
        private readonly ILocationService locationService;

        public AddressPanelViewModel(IMapPageView mapPageView, ILocationService locationService)
        {
            this.mapPageView = mapPageView;
            this.locationService = locationService;
        }

        public async void PlaceOnMap()
        {
            try
            {
                await SetPosition();
                mapPageView.PlaceOnMapClicked(position);
            }
            catch
            {
                mapPageView.DisplayProgressMessage("An unexpected error occured during placing the pin on the map.");
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
                mapPageView.DisplayProgressMessage("An unexpected error occured during showing the address on the map.");
            }
        }

        public async Task UpdateUserPositionAsync()
        {
            var location = locationService.Location;

            if (location != null)
            {
                position = new Position(location.Latitude, location.Longitude);

                Longitude = position.Longitude.ToString();
                Latitude = position.Latitude.ToString();

                await SetAddressAsync(position);
            }
            else
            {
                mapPageView.DisplayProgressMessage("An unexpected error occured during setting the address. Do you have internet connection?");
            }
        }

        private async Task SetPosition()
        {
            try
            {
                double latitude = double.Parse(this.latitude);
                double longitude = double.Parse(this.longitude);

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
            catch
            {
                mapPageView.DisplayProgressMessage("An unexpected error occured during getting the location from the Internet. The device might be working offline.");
            }
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

        public string Longitude 
        { 
            get => longitude; 
            set 
            { 
                longitude = value; 
                OnPropertyChanged("Longitude"); 
            } 
        }

        public string Latitude 
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
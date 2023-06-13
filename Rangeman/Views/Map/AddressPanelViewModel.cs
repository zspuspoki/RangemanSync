using Mapsui.UI.Forms;
using Rangeman.Services.PhoneLocation;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;
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
        private string latitudeLongitude;
        private bool isLatitudeLongitudeInvalid = false;
        private bool islatitudeLongitudeValid = false;
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
                if(UseGPSCoordinatesInsteadOfAddress && !IsLatitudeLongitudeValid)
                {
                    return;
                }

                await SetPosition();
                mapPageView.PlaceOnMapClicked(position);
                LatitudeLongitude = $"{position.Latitude.ToString(CultureInfo.InvariantCulture)}, {position.Longitude.ToString(CultureInfo.InvariantCulture)}";
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
                if (UseGPSCoordinatesInsteadOfAddress && !IsLatitudeLongitudeValid)
                {
                    return;
                }

                await SetPosition();
                mapPageView.ShowOnMap(position);
                LatitudeLongitude = $"{position.Latitude.ToString(CultureInfo.InvariantCulture)}, {position.Longitude.ToString(CultureInfo.InvariantCulture)}";
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

                LatitudeLongitude = $"{position.Latitude.ToString(CultureInfo.InvariantCulture)}, {position.Longitude.ToString(CultureInfo.InvariantCulture)}";

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
                double latitude = 0;  
                double longitude = 0;

                if (IsLatitudeLongitudeValid)
                {
                    var latitudelongitudeSplitted = LatitudeLongitude.Split(',');

                    if (latitudelongitudeSplitted.Length == 2)
                    {
                        if (double.TryParse(latitudelongitudeSplitted[0], out var latitudeEntered) &&
                            double.TryParse(latitudelongitudeSplitted[1], out var longitudeEntered))
                        {
                            latitude = latitudeEntered;
                            longitude = longitudeEntered;
                        }
                    }
                }

                if (!UseGPSCoordinatesInsteadOfAddress)
                {
                    var location = (await Geocoding.GetLocationsAsync($"{street}, {city}, {country}")).FirstOrDefault();

                    if (location == null) return;

                    latitude = location.Latitude;
                    longitude = location.Longitude;
                }

                if (!IsLatitudeLongitudeValid)
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

        public bool IsLatitudeLongitudeValid 
        {
            get => islatitudeLongitudeValid;
            set
            {
                islatitudeLongitudeValid = value;
                IsLatitudeLongitudeInvalid = !value;
            }
        }

        public string LatitudeLongitude
        {
            get => latitudeLongitude;
            set
            {
                latitudeLongitude = value;
                OnPropertyChanged("LatitudeLongitude");
            }
        }

        public bool IsLatitudeLongitudeInvalid 
        { 
            get => isLatitudeLongitudeInvalid; 
            set
            {
                isLatitudeLongitudeInvalid = value;
                OnPropertyChanged("IsLatitudeLongitudeInvalid");
            }
        }
    }
}
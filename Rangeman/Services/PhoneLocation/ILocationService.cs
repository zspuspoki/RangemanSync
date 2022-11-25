using Xamarin.Essentials;

namespace Rangeman.Services.PhoneLocation
{
    public interface ILocationService
    {
        Location Location { get; set; }

        void GetPhoneLocation();
    }
}
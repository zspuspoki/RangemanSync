using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Rangeman.Services.PhoneLocation
{
    public class LocationService : ILocationService
    {
        private readonly ILogger<LocationService> logger;

        public Location Location { get; set; }

        public LocationService(ILogger<LocationService> logger)
        {
            this.logger = logger;
            GetPhoneLocation();
        }

        public void GetPhoneLocation()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(11000);

            Task.Factory.StartNew(() => FillOutLocation(cancellationTokenSource.Token));
        }

        private async Task FillOutLocation(CancellationToken cancellationToken)
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location != null)
                {
                    Location = location;
                    return;
                }

                logger.LogInformation("Before executing GetLocationAsync");
                var locationRequest = new GeolocationRequest(GeolocationAccuracy.Medium, new TimeSpan(0, 0, 10));
                location = await Geolocation.GetLocationAsync(locationRequest, cancellationToken);
                if (location != null)
                {
                    Location = location;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting location from GetLastKnownLocationAsync or GetLocationAsync");
            }
        }
    }
}
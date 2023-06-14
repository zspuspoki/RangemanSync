using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Behaviors.Internals;

namespace Rangeman
{
    public class LatitudeLongitudeValidationBehavior : ValidationBehavior
    {
        protected override ValueTask<bool> ValidateAsync(object value, CancellationToken token)
        {
            //NumericValidationBehavior
            string text = value as string;
            if (text == null)
            {
                return new ValueTask<bool>(result: false);
            }

            if(!text.Contains(","))
            {
                return new ValueTask<bool>(result: false);
            }

            var splittedText = text.Split(',');
            if(splittedText.Length!= 2)
            {
                return new ValueTask<bool>(result: false);
            }

            if (!double.TryParse(splittedText[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double latitude))
            {
                return new ValueTask<bool>(result: false);
            }

            if(!double.TryParse(splittedText[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double longitude))
            {
                return new ValueTask<bool>(result: false);
            }

            if(!(latitude>=-90 && latitude<=90))
            {
                return new ValueTask<bool>(result: false);
            }

            if (!(longitude >= -180 && longitude <= 180))
            {
                return new ValueTask<bool>(result: false);
            }

            return new ValueTask<bool>(result: true);
        }
    }
}
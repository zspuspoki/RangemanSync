using Rangeman.Services.LicenseDistributor;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace RangemanSync.Android.Services
{   

    public class LicenseInfoDistributorService : ILicenseDistributor, ILicenseDistributorErrorHandler
    {
        private LicenseValidity validity = LicenseValidity.Invalid;
        private string errorCode = "";

        public LicenseValidity GetValidity()
        {
            return validity;
        }

        public void setErrorCode(string errorCode)
        {
            MessagingCenter.Send<ILicenseDistributorErrorHandler>(this, errorCode);
        }

        public void SetValidity(LicenseValidity licenseValidity)
        {
            validity = licenseValidity;

            MessagingCenter.Send<ILicenseDistributor>(this, validity.ToString());
        }
    }
}
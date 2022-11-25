using Rangeman.Services.LicenseDistributor;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace RangemanSync.Android.Services
{   

    public class LicenseInfoDistributorService : ILicenseDistributor
    {
        public LicenseValidity Validity { get; set; } = LicenseValidity.Valid;
        public string ErrorCode { get; set; } = "";

        public void setErrorCode(string errorCode)
        {
            ErrorCode = errorCode;
            MessagingCenter.Send<ILicenseDistributor>(this, DistributorMessages.AppErrorReceived.ToString());
        }

        public void SetValidity(LicenseValidity licenseValidity)
        {
            Validity = licenseValidity;

            if(Validity == LicenseValidity.Valid)
            {
                ErrorCode = null;
            }

            MessagingCenter.Send<ILicenseDistributor>(this, DistributorMessages.LicenseResultReceived.ToString());
        }
    }
}
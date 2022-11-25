namespace Rangeman.Services.LicenseDistributor
{
    public enum DistributorMessages
    {
        AppErrorReceived, LicenseResultReceived
    }

    public enum LicenseValidity
    {
        Valid, Invalid
    }

    public interface ILicenseDistributor
    {
        LicenseValidity Validity { get; set; }
        string ErrorCode { get; set; }
        void SetValidity(LicenseValidity licenseValidity);

        void setErrorCode(string errorCode);
    }
}
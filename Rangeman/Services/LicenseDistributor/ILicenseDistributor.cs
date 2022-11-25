namespace Rangeman.Services.LicenseDistributor
{
    public enum LicenseValidity
    {
        Valid, Invalid
    }

    public interface ILicenseDistributor
    {
        LicenseValidity GetValidity();

        void SetValidity(LicenseValidity licenseValidity);
    }
}
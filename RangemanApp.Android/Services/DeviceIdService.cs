using Android.Content;
using Rangeman.Services.DeviceId;
using System;

namespace RangemanSync.Android.Services
{
    internal class DeviceIdService : IDeviceIdService
    {
        private readonly ContentResolver contentResolver;

        public DeviceIdService(ContentResolver? contentResolver)
        {
            this.contentResolver = contentResolver;
        }
        public string GetDeviceId()
        {
            try
            {
                return global::Android.Provider.Settings.Secure.GetString(contentResolver, global::Android.Provider.Settings.Secure.AndroidId);
            }
            catch
            {
                return "Unable to retrieve Device ID";
            }
        }
    }
}
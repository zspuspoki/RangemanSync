using Rangeman.Services.SharedPreferences;
using Xamarin.Essentials;

namespace employeeID.Droid
{
    public class SharedPreferencesService : ISharedPreferencesService
    {
        public string GetValue(string key, string defaultValue)
        {
            return Preferences.Get(key, defaultValue);
        }

        public void SetValue(string key, string value)
        {
            Preferences.Set(key, value);
        }
    }
}
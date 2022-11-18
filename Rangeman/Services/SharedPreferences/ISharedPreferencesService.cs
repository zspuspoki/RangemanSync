namespace Rangeman.Services.SharedPreferences
{
    public interface ISharedPreferencesService
    {
        void SetValue(string key, string value);
        string GetValue(string key, string defaultValue);
    }
}
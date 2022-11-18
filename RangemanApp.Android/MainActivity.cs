using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Essentials;
using Rangeman;
using nexus.protocols.ble;
using Android.Content;
using Rangeman.Services.SharedPreferences;
using Android.Media;

namespace employeeID.Droid
{
    [Activity(Label = "employeeID", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private ISharedPreferencesService preferencesService;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            BluetoothLowEnergyAdapter.Init(this);
            Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            preferencesService = new SharedPreferencesService();

            var setup = new Setup(ApplicationContext, this, preferencesService);
            LoadApplication(new App(setup.Configuration, setup.DependencyInjection));

            await Permissions.RequestAsync<AppPermissions>();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            using (System.IO.Stream stream = this.ContentResolver.OpenOutputStream(data.Data, "w"))
            {
                using (var javaStream = new Java.IO.BufferedOutputStream(stream))
                {
                    string gpx = preferencesService.GetValue(Constants.PrefKeyGPX, "");

                    if (!string.IsNullOrEmpty(gpx))
                    {
                        var gpxBytes = System.Text.Encoding.Unicode.GetBytes(gpx);
                        javaStream.Write(gpxBytes, 0, gpxBytes.Length);
                    }

                    javaStream.Flush();
                    javaStream.Close();
                }
            }
        }
    }
}
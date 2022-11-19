using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Essentials;
using Rangeman;
using nexus.protocols.ble;
using Android.Content;
using Rangeman.Services.SharedPreferences;
using System.Threading.Tasks;
using System.Collections.Generic;
using static Xamarin.Essentials.Permissions;
using Xamarin.Forms.Internals;

namespace employeeID.Droid
{
    [Activity(Label = "RangemanSync", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private ISharedPreferencesService preferencesService;
        private List<string> appPermissions = new List<string>();

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            BluetoothLowEnergyAdapter.Init(this);
            Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            await CheckPermissions();

            preferencesService = new SharedPreferencesService();

            var setup = new Setup(ApplicationContext, this, preferencesService);
            LoadApplication(new App(setup.Configuration, setup.DependencyInjection));

        }

        private async Task CheckPermissions()
        {
            BasePlatformPermission locatonWhenInUsePermission = new Permissions.LocationWhenInUse();
            BasePlatformPermission bluetoothPermissons = new BluetoothPermissions();

            locatonWhenInUsePermission.RequiredPermissions.ForEach(p => appPermissions.Add(p.androidPermission));
            bluetoothPermissons.RequiredPermissions.ForEach(p => appPermissions.Add(p.androidPermission));

            var locationPermissionStatus = await CheckAndRequestLocationPermission();
            var bluetoothPermissionStatus = await Permissions.RequestAsync<BluetoothPermissions>();

            if (locationPermissionStatus != PermissionStatus.Granted)
            {
                DisplayErrorDialog("This app needs location permisson to work. Please enable it.");
            }

            if (bluetoothPermissionStatus != PermissionStatus.Granted)
            {
                DisplayErrorDialog("This app needs location permisson to work. Please enable it.");
            }
        }

        private void DisplayErrorDialog(string message)
        {
            var alert = new AlertDialog.Builder(this);
            alert.SetTitle("Warning");
            alert.SetMessage(message);
            alert.SetPositiveButton("OK", (s, a) =>
            {
                FinishAndRemoveTask();
                return;
            });

            Dialog dialog = alert.Create();
            dialog.Show();
        }

        public async Task<PermissionStatus> CheckAndRequestLocationPermission()
        {
            PermissionStatus status = PermissionStatus.Unknown;

            status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
                return status;

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            return status;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            for (int i=0;i<permissions.Length;i++)
            {
                if (appPermissions.Contains(permissions[i]) && grantResults[i] == Permission.Denied)
                {
                    DisplayErrorDialog("Please enable the required permissions!");
                    return;
                }
            }

            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                if (requestCode == ActivityRequestCode.SaveGPXFile)
                {
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
    }
}
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
using AndroidContent = Android.Content;
using System;
using Environment = System.Environment;
using System.IO;
using Google.Android.Vending.Licensing;
using Rangeman.Services.LicenseDistributor;
using RangemanSync.Android.Services;

namespace RangemanSync.Android
{
    [Activity(Label = "RangemanSync", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, ILicenseCheckerCallback
    {
        private ISharedPreferencesService preferencesService;
        private readonly ILicenseDistributor licenseDistributor = new LicenseInfoDistributorService();
        private List<string> appPermissions = new List<string>();

        #region License checking related fields
        /// <summary>
        /// Your Base 64 public key
        /// </summary>
        private const string Base64PublicKey =
            "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA96TCUr/Rhx/fcIVcCrWTz0FKvI+hZ" +
            "ICb/yXaxNPhSWeo7TROB+Op5wKhdmjsaSvbi/v75RgyikS/HrSKvQCqwix6b3IgjIu8iGYYZz" +
            "2ieoFMVt39WFP20fSfjNoBr0KJOsoIAso6zF845ZtIE+3vJFg4z/tTe/jPgi73AYJS6RnUO2p" +
            "C2tzeGVe+TQemhPUfFWAczunpAoT8ioBCYzK1FzTc1uyAFMh8riijrKDXbQd42nByJq3SSjJi" +
            "yx/5pcMMj2kWvuJjD5ugk0X10jEfwptVQytXOAvMPhbyvJ2yNN6Ha9ZUHIawXC+JyCr9bvMAo" +
            "KIFTqzqLYfpX10feYTDsQIDAQAB";

        // Generate your own 20 random bytes, and put them here.
        private static readonly byte[] Salt = new byte[]
            { 46, 65, 30, 128, 103, 57, 74, 64, 51, 88, 95, 45, 77, 117, 36, 113, 11, 32, 64, 89 };

        private LicenseChecker checker;
        #endregion

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            BluetoothLowEnergyAdapter.Init(this);
            Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            await CheckPermissions();

            preferencesService = new SharedPreferencesService();

            var setup = new Setup(ApplicationContext, this, preferencesService);
            LoadApplication(new App(setup.Configuration, setup.DependencyInjection));

            ConfigureLicenseChecking();
        }

        #region License checking related methods
        private void ConfigureLicenseChecking()
        {
            // Try to use more data here. ANDROID_ID is a single point of attack.
            string deviceId = global::Android.Provider.Settings.Secure.GetString(ContentResolver, global::Android.Provider.Settings.Secure.AndroidId);

            // Construct the LicenseChecker with a policy.
            var obfuscator = new AESObfuscator(Salt, PackageName, deviceId);
            var policy = new ServerManagedPolicy(this, obfuscator);
            checker = new LicenseChecker(this, policy, Base64PublicKey);

            DoCheckLicense();
        }

        private void DoCheckLicense()
        {
            checker.CheckAccess(this);
        }

        public void Allow([GeneratedEnum] PolicyResponse reason)
        {
            licenseDistributor.SetValidity(LicenseValidity.Valid);
        }

        public void ApplicationError([GeneratedEnum] LicenseCheckerErrorCode errorCode)
        {
            licenseDistributor.setErrorCode(errorCode.ToString());
        }

        public void DontAllow([GeneratedEnum] PolicyResponse reason)
        {
            licenseDistributor.SetValidity(LicenseValidity.Invalid);
        }

        #endregion

        #region Error handling
        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var newExc = new Exception("TaskSchedulerOnUnobservedTaskException", e.Exception);
            LogUnhandledException(newExc);
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var newExc = new Exception("CurrentDomainOnUnhandledException", e.ExceptionObject as Exception);
            LogUnhandledException(newExc);
        }

        internal static void LogUnhandledException(Exception exception)
        {
            try
            {
                const string errorFileName = "Fatal.log";
                var docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var logsPath = Path.Combine(docsPath, Constants.LogSubFolder);

                var errorFilePath = Path.Combine(logsPath, errorFileName);
                var errorMessage = String.Format("Time: {0}\r\nError: Unhandled Exception\r\n{1}",
                DateTime.Now, exception.ToString());
                File.WriteAllText(errorFilePath, errorMessage);

            }
            catch
            {
                // just suppress any error logging exceptions
            }
        }
        #endregion

        #region Permission checking

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
                Process.KillProcess(Process.MyPid());
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

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] AndroidContent.PM.Permission[] grantResults)
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
        #endregion

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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            checker.OnDestroy();
        }
    }
}
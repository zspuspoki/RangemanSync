using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Essentials;
using Rangeman;
using Android.Content;
using Rangeman.Services.SharedPreferences;
using System.Threading.Tasks;
using AndroidContent = Android.Content;
using System;
using Environment = System.Environment;
using System.IO;
using Rangeman.Services.LicenseDistributor;
using RangemanSync.Android.Services;
using Constants = Rangeman.Constants;
using Android;
using AndroidX.Core.Content;
using AndroidX.Core.App;
using Google.Android.Vending.Licensing;

namespace RangemanSync.Android
{
    [Activity(Label = "RangemanSync", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, ActivityCompat.IOnRequestPermissionsResultCallback, ILicenseCheckerCallback
    {
        private ISharedPreferencesService preferencesService;
        private readonly ILicenseDistributor licenseDistributor = new LicenseInfoDistributorService();
        private const int BLUETOOTH_PERMISSION_REQUEST = 1;
        private const int LOCATION_PERMISSION_REQUEST = 2;

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

        #region EULA checking related fields
        private const string EULA_HAS_BEEN_READ = nameof(EULA_HAS_BEEN_READ);
        private const string EULA_HAS_BEEN_ACCEPTED = nameof(EULA_HAS_BEEN_ACCEPTED);
        private const string EULA_URL = "https://github.com/szilamer007/RangemanSync/blob/main/EULA.md";
        #endregion

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            CheckPermissions_Compat();

            preferencesService = new SharedPreferencesService();
            var deviceIdService = new DeviceIdService(ContentResolver);

            var setup = new Setup(ApplicationContext, this, preferencesService, 
                licenseDistributor, deviceIdService);
            LoadApplication(new App(setup.Configuration, setup.DependencyInjection));

            var eulaHasBeenAccepted = await UserHasAcceptedEULA();

            if (eulaHasBeenAccepted)
            {
                ConfigureLicenseChecking();
            }
        }

        #region EULA checking
        private Task<bool> UserHasAcceptedEULA()
        {            
            if (!Xamarin.Forms.Application.Current.Properties.ContainsKey(EULA_HAS_BEEN_READ))
            {
                DisplayYesNoDialog($"The EULA has to be read to start using this application. Would you like to read it now on {EULA_URL}",
                    async () =>
                    {                        
                        await OpenEULAUrl();
                        Xamarin.Forms.Application.Current.Properties[EULA_HAS_BEEN_READ] = "true";
                        await Xamarin.Forms.Application.Current.SavePropertiesAsync();
                        Process.KillProcess(Process.MyPid());
                    },
                    () =>
                    {
                        Process.KillProcess(Process.MyPid());
                    });
            }
            else
            {
                if (!Xamarin.Forms.Application.Current.Properties.ContainsKey(EULA_HAS_BEEN_ACCEPTED))
                {
                    TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
                    DisplayYesNoDialog($"The EULA has to be accepted. Do you accept the EULA on {EULA_URL} ?",
                        async () =>
                        {
                            Xamarin.Forms.Application.Current.Properties[EULA_HAS_BEEN_ACCEPTED] = "true";
                            await Xamarin.Forms.Application.Current.SavePropertiesAsync();
                            taskCompletionSource.SetResult(true);
                        },
                        () => 
                        {
                            Process.KillProcess(Process.MyPid());
                        });

                    return taskCompletionSource.Task;
                }
                else
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        private async Task OpenEULAUrl()
        {
            try
            {
                Uri uri = new Uri(EULA_URL);
                await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                LogUnhandledException(ex);
            }

        }
        #endregion

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

        private void CheckPermissions_Compat()
        {
            var locationPermissions = new string[] { Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation };

            if (DeviceInfo.Version.Major > 11)
            {
                var bluetoothPermissions = new string[] { "android.permission.BLUETOOTH_SCAN", "android.permission.BLUETOOTH_CONNECT" };

                if (ContextCompat.CheckSelfPermission(this, bluetoothPermissions[0]) != (int)Permission.Granted ||
                    ContextCompat.CheckSelfPermission(this, bluetoothPermissions[1]) != (int)Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this, bluetoothPermissions, BLUETOOTH_PERMISSION_REQUEST);
                }
            }

            if (ContextCompat.CheckSelfPermission(this, locationPermissions[0]) != (int)Permission.Granted ||
                ContextCompat.CheckSelfPermission(this, locationPermissions[1]) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, locationPermissions, LOCATION_PERMISSION_REQUEST);
            }
        }

        private void DisplayYesNoDialog(string message, Action yesButtonMethod, Action noButtonMethod)
        {
            var alert = new AlertDialog.Builder(this);
            alert.SetTitle("Warning");
            alert.SetMessage(message);
            alert.SetCancelable(false);
            alert.SetPositiveButton("Yes", (s, a) =>
            {
                if(yesButtonMethod != null)
                {
                    yesButtonMethod();
                }
            });

            alert.SetNegativeButton("No", (s, a) =>
            {
                if (noButtonMethod != null)
                {
                    noButtonMethod();
                }
            });

            Dialog dialog = alert.Create();
            dialog.Show();
        }


        private void DisplayErrorDialog(string message)
        {
            var alert = new AlertDialog.Builder(this);
            alert.SetTitle("Warning");
            alert.SetMessage(message);
            alert.SetCancelable(false);
            alert.SetPositiveButton("OK", (s, a) =>
            {
                Process.KillProcess(Process.MyPid());
                return;
            });

            Dialog dialog = alert.Create();
            dialog.Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] AndroidContent.PM.Permission[] grantResults)
        {
            switch(requestCode)
            {
                case LOCATION_PERMISSION_REQUEST:
                case BLUETOOTH_PERMISSION_REQUEST:
                    foreach(var grantResult in grantResults)
                    {
                        if (grantResult == Permission.Denied)
                        {
                            var permissionType = (requestCode == LOCATION_PERMISSION_REQUEST) ? "Location" : "Bluetooth";
                            DisplayErrorDialog($"{permissionType} permissions should be granted to use this. Press OK to exit.");
                        }
                    }
                    break;
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
                    using (Stream stream = this.ContentResolver.OpenOutputStream(data.Data, "w"))
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
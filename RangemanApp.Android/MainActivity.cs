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
using AndroidContent = Android.Content;
using System;
using Environment = System.Environment;
using System.IO;
using Rangeman.Services.LicenseDistributor;
using RangemanSync.Android.Services;
using NetLicensingClient.Entities;
using NetLicensingClient;
using Constants = Rangeman.Constants;
using Android;
using AndroidX.Core.Content;
using AndroidX.Core.App;

namespace RangemanSync.Android
{
    [Activity(Label = "RangemanSync", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, ActivityCompat.IOnRequestPermissionsResultCallback
    {
        private ISharedPreferencesService preferencesService;
        private readonly ILicenseDistributor licenseDistributor = new LicenseInfoDistributorService();
        private const int BLUETOOTH_PERMISSION_REQUEST = 1;
        private const int LOCATION_PERMISSION_REQUEST = 2;

        #region License checking related fields
        private const string LICENSING_APIKEY = "7b9b238f-85bf-4c7c-8702-f4699f795091";
        private const string LICENSING_PRODUCT_NUMBER = "PU6XMYAZC";
        private const string LICENSING_PRODUCT_MODULE_NUMBER = "MR6YEGKQF";
        private const string SECURE_STORAGE_LICENSE_CHECK_DATE = "LicenseCheckDate";
        private const string SECURE_STORAGE_LICENSE_EXPIRATION_DATE = "LicenseExpirationDate";
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
                var licenseCheckingTask = Task.Run(() => StartLicenseChecking());
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
        private async void StartLicenseChecking()
        {
            try
            {
                NetLicensingClient.Context context;
                string deviceId, licenseIsValid, licenseExpires;
                var licenseIsValidInSecureStorage = await CheckLicenseValidityInSecureStorage();

                if(licenseIsValidInSecureStorage)
                {
                    licenseDistributor.SetValidity(LicenseValidity.Valid);
                    return;
                }

                CheckLicenseValidityOnServer(out context, out deviceId, out licenseIsValid, out licenseExpires);

                if (licenseIsValid == "false")
                {
                    licenseDistributor.SetValidity(LicenseValidity.Invalid);
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        DisplayYesNoDialog("You don't have a valid license. Would you like to acquire it in the license shop ?",
                            async () =>
                            {
                                await OpenBrowserWithLicenseShop(context, deviceId);
                                Process.KillProcess(Process.MyPid());
                            }, null);
                    });
                }
                else
                {
                    SetLicenseValidityInSecureStorage(licenseExpires);
                    licenseDistributor.SetValidity(LicenseValidity.Valid);
                }
            }
            catch (Exception ex)
            {
                licenseDistributor.setErrorCode($"An unexpected error occured during checking the license: {ex.Message}");
            }
        }

        private static async Task OpenBrowserWithLicenseShop(NetLicensingClient.Context context, string deviceId)
        {
            Token newToken = new Token();
            newToken.tokenType = NetLicensingClient.Entities.Constants.Token.TYPE_SHOP;
            newToken.tokenProperties.Add(NetLicensingClient.Entities.Constants.Licensee.LICENSEE_NUMBER, deviceId);
            Token shopToken = TokenService.create(context, newToken);
            var shopURL = shopToken.tokenProperties["shopURL"];
            var uri = new Uri(shopURL);
            try
            {
                await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
                Process.KillProcess(Process.MyPid());
            }
            catch (Exception ex)
            {
                LogUnhandledException(ex);
            }
        }

        private async Task<bool> CheckLicenseValidityInSecureStorage()
        {
            var licenseIsValid = true;
            string licenseExpirationString = "";
            string licenseLastCheckedString = "";
            try
            {
                licenseLastCheckedString = await SecureStorage.GetAsync(SECURE_STORAGE_LICENSE_CHECK_DATE);
                licenseExpirationString = await SecureStorage.GetAsync(SECURE_STORAGE_LICENSE_EXPIRATION_DATE);
            }
            catch
            {
                licenseIsValid = false;
            }

            if(DateTime.TryParse(licenseExpirationString, out var licenseExpiration) &&
               DateTime.TryParse(licenseLastCheckedString, out var licenseLastChecked))
            {
                if(DateTime.Now> licenseExpiration)
                {
                    return false;
                }

                var timeElapsedSinceLastLicenseChecking = licenseLastChecked - DateTime.Now;
                if(timeElapsedSinceLastLicenseChecking.TotalDays >= 20)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return licenseIsValid;
        }

        private void CheckLicenseValidityOnServer(out NetLicensingClient.Context context, out string deviceId, 
            out string licenseIsValid, out string licenseExpires)
        {
            ValidationParameters validationParameters = new ValidationParameters();
            validationParameters.setProductNumber(LICENSING_PRODUCT_NUMBER);

            context = new NetLicensingClient.Context();
            context.securityMode = NetLicensingClient.SecurityMode.APIKEY_IDENTIFICATION;
            context.apiKey = LICENSING_APIKEY;
            deviceId = global::Android.Provider.Settings.Secure.GetString(ContentResolver, global::Android.Provider.Settings.Secure.AndroidId);
            ValidationResult validationResult = LicenseeService.validate(context, deviceId, validationParameters);
            var validations = validationResult.getValidations();
            var licenseValidityForProductModule = validations[LICENSING_PRODUCT_MODULE_NUMBER];
            //TODO: expires
            licenseIsValid = licenseValidityForProductModule.properties["valid"].value;
            licenseExpires = "";

            if(licenseIsValid == "true")
            {
                licenseExpires = licenseValidityForProductModule.properties["expires"].value;
            }
        }

        private async void SetLicenseValidityInSecureStorage(string expirationDate)
        {
            try
            {
                await SecureStorage.SetAsync(SECURE_STORAGE_LICENSE_CHECK_DATE, DateTime.Now.ToString());
                await SecureStorage.SetAsync(SECURE_STORAGE_LICENSE_EXPIRATION_DATE, expirationDate);
            }
            catch (Exception ex)
            {
                LogUnhandledException(ex);
            }
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
        }
    }
}
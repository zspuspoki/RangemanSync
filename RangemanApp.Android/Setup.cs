using Android.Content;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Rangeman;
using Rangeman.Services.LicenseDistributor;
using Rangeman.Services.SharedPreferences;
using System;
using Xamarin.Forms.Platform.Android;

namespace RangemanSync.Android
{
    public class Setup
    {
        private readonly Context context;
        private readonly FormsAppCompatActivity mainActivity;
        private readonly ISharedPreferencesService sharedPreferencesService;
        private readonly ILicenseDistributor licenseDistributor;

        public Setup(Context context, FormsAppCompatActivity mainActivity, 
            ISharedPreferencesService sharedPreferencesService, ILicenseDistributor licenseDistributor)
        {
            this.context = context;
            this.mainActivity = mainActivity;
            this.sharedPreferencesService = sharedPreferencesService;
            this.licenseDistributor = licenseDistributor;
        }

        public Action<ConfigurationBuilder> Configuration =>
                    (builder) =>
                    {
                        builder.AddJsonFile(new EmbeddedFileProvider(typeof(Setup).Assembly, typeof(Setup).Namespace), "appsettings.json", false,
                            false);
                    };

        public Action<IServiceCollection, IConfigurationRoot> DependencyInjection =>
            (serviceCollection, configurationRoot) =>
            {
                serviceCollection.AddSingleton(this.context);
                serviceCollection.AddSingleton(mainActivity);
                serviceCollection.AddSingleton<ISaveGPXFileService, SaveGPXFileService>();
                serviceCollection.AddSingleton<ISharedPreferencesService>(sharedPreferencesService);
                serviceCollection.AddSingleton<ILicenseDistributor>(licenseDistributor);
            };
    }
}
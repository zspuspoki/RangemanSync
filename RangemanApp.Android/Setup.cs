using Android.Content;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Rangeman;
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

        public Setup(Context context, FormsAppCompatActivity mainActivity, 
            ISharedPreferencesService sharedPreferencesService)
        {
            this.context = context;
            this.mainActivity = mainActivity;
            this.sharedPreferencesService = sharedPreferencesService;
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
                serviceCollection.AddSingleton<ISharedPreferencesService>(sharedPreferencesService);
            };
    }
}
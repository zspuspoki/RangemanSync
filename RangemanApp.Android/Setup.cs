using Android.Content;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using Xamarin.Forms.Platform.Android;

namespace RangemanSync.Android
{
    public class Setup
    {
        private readonly Context context;
        private readonly FormsAppCompatActivity mainActivity;

        public Setup(Context context, FormsAppCompatActivity mainActivity)
        {
            this.context = context;
            this.mainActivity = mainActivity;
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
            };
    }
}
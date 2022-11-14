using Android.Content;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;

namespace employeeID.Droid
{
    public class Setup
    {
        private readonly Context context;

        public Setup(Android.Content.Context context)
        {
            this.context = context;
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
            };
    }
}
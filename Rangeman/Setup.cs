using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using nexus.protocols.ble;
using Rangeman.Services.BluetoothConnector;
using Rangeman.Views.Download;
using Rangeman.Views.Map;
using Serilog;
using System;
using System.IO;
using Xamarin.Forms;

namespace Rangeman
{
    public static class Setup
    {
        public static ConfigurationBuilder Configuration => new ConfigurationBuilder();

        public static IServiceCollection DependencyInjection => new ServiceCollection();

        public static IServiceCollection AddViewModels<T>(this IServiceCollection serviceCollection)
        {
            // https://github.com/khellang/Scrutor can simplify things when setting up service registrations
            return serviceCollection.Scan(selector =>
                selector.FromAssemblies(typeof(T).Assembly)
                    .AddClasses(filter => filter.InNamespaceOf(typeof(T)))
                    .AsSelf()
                    .WithTransientLifetime());
        }

        public static IServiceCollection AddViews(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddViewSingleton<ConfigPage, ConfigPageViewModel>();

            serviceCollection.AddSingleton<IDownloadPageView, DownloadPage>();
            serviceCollection.AddViewSingleton<IDownloadPageView, DownloadPage, DownloadPageViewModel>();

            serviceCollection.AddSingleton<IMapPageView, MapPage>();
            serviceCollection.AddViewSingleton<IMapPageView, MapPage, MapPageViewModel>();

            return serviceCollection;
        }

        public static IServiceCollection ConfigureLogging(this IServiceCollection serviceCollection, IConfigurationRoot configurationRoot)
        {
            return serviceCollection.AddLogging(builder =>
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                var loggerConfiguration = new LoggerConfiguration()
                    .ReadFrom.Configuration(configurationRoot.GetSection("Logging"))
                    .WriteTo.File(Path.Combine(path, Constants.LogSubFolder, "Log.log"), 
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} ({SourceContext}) {Exception}{NewLine}");

                builder.AddSerilog(loggerConfiguration.CreateLogger());
            });
        }

        public static ConfigurationBuilder ConfigureRangemanProject(this ConfigurationBuilder builder)
        {
            builder.AddJsonFile(new EmbeddedFileProvider(typeof(Setup).Assembly), "appsettings.json", false, false);

            return builder;
        }

        public static IServiceCollection ConfigureRangemanProject(this IServiceCollection serviceCollection,
            IConfigurationRoot configurationRoot)
        {
            serviceCollection.AddViewModels<DownloadPageViewModel>();
            serviceCollection.AddSingleton<MapPageViewModel>();
            serviceCollection.AddViewModels<NodesViewModel>();
            serviceCollection.AddViewModels<AddressPanelViewModel>();
            serviceCollection.AddSingleton<ConfigPageViewModel>();

            serviceCollection.AddSingleton<BluetoothConnectorService>((ctx) => 
            {
                var context = ctx.GetService<Android.Content.Context>();
                var ble = BluetoothLowEnergyAdapter.ObtainDefaultAdapter(context);
                var logger = ctx.GetService<Microsoft.Extensions.Logging.ILogger<BluetoothConnectorService>>();

                return new BluetoothConnectorService(ble, logger);
            });

            serviceCollection.AddViews();
            serviceCollection.AddSingleton<AppShellViewModel>();

            return serviceCollection;
        }

        public static ConfigurationBuilder ConfigureRangemanAppProject(this ConfigurationBuilder builder,
            Action<ConfigurationBuilder> configure)
        {
            configure(builder);

            return builder;
        }

        public static IServiceCollection ConfigureRangemanAppProject(this IServiceCollection serviceCollection,
            IConfigurationRoot configurationRoot, Action<IServiceCollection, IConfigurationRoot> configure)
        {
            configure(serviceCollection, configurationRoot);

            return serviceCollection;
        }

        private static IServiceCollection AddViewSingleton<TView, TViewModel>(this IServiceCollection serviceCollection) where TView : Page
        {
            return serviceCollection.AddSingleton<TView>(serviceProvider =>
            {
                TView view = ActivatorUtilities.CreateInstance<TView>(serviceProvider);

                // Autobind specified view model
                view.BindingContext = serviceProvider.GetRequiredService<TViewModel>();

                view.Appearing += (sender, args) => (((BindableObject)sender).BindingContext as IPageLifeCycleAware)?.OnAppearing();
                view.Disappearing += (sender, args) => (((BindableObject)sender).BindingContext as IPageLifeCycleAware)?.OnDisappearing();

                return view;
            });
        }

        private static IServiceCollection AddViewSingleton<TViewInterface, TView, TViewModel>(this IServiceCollection serviceCollection) where TView : Page
        {
            return serviceCollection.AddSingleton<TView>(serviceProvider =>
            {
                var viewInterface= serviceProvider.GetRequiredService<TViewInterface>();
                TView view = (TView)Convert.ChangeType(viewInterface, typeof(TView));

                view.BindingContext = serviceProvider.GetRequiredService<TViewModel>();

                view.Appearing += (sender, args) => (((BindableObject)sender).BindingContext as IPageLifeCycleAware)?.OnAppearing();
                view.Disappearing += (sender, args) => (((BindableObject)sender).BindingContext as IPageLifeCycleAware)?.OnDisappearing();

                return view;
            });
        }

    }
}
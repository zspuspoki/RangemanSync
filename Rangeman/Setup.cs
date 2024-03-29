﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using nexus.protocols.ble;
using Rangeman.Services.BluetoothConnector;
using Rangeman.Services.PhoneLocation;
using Rangeman.Views.Common;
using Rangeman.Views.Coordinates;
using Rangeman.Views.Download;
using Rangeman.Views.Map;
using Rangeman.Views.Tide;
using Rangeman.Views.Time;
using Serilog;
using Serilog.Exceptions;
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

            serviceCollection.AddViewSingleton<CoordinatesPage, CoordinatesViewModel>();

            serviceCollection.AddViewSingleton<CustomTime, CustomTimeViewModel>();

            serviceCollection.AddViewSingleton<NTPTime, NTPTimeViewModel>();

            serviceCollection.AddViewSingleton<BackgroundTimeSyncLog, BackgroundTimeSyncLogViewModel>();

            serviceCollection.AddViewSingleton<Tide, TideViewModel>();

            serviceCollection.AddSingleton<IDownloadPageView, DownloadPage>();
            serviceCollection.AddViewSingleton<IDownloadPageView, DownloadPage, DownloadPageViewModel>();

            serviceCollection.AddSingleton<IMapPageView, MapPage>();
            serviceCollection.AddViewSingleton<IMapPageView, MapPage, MapPageViewModel>();

            return serviceCollection;
        }

        public static IServiceCollection ConfigureLogging(this IServiceCollection serviceCollection, IConfigurationRoot configurationRoot)
        {
            const string TimeSyncServicePropertyName = "BackgroundTimeSyncService";

            return serviceCollection.AddLogging(builder =>
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                var loggerConfiguration = new LoggerConfiguration()
                    .ReadFrom.Configuration(configurationRoot)
                    .Enrich.WithExceptionDetails()
                    .Enrich.FromLogContext()
#if DEBUG
                    .WriteTo.Debug()
#endif
                    .WriteTo.Logger(l =>
                    {
                        l.WriteTo.File(Path.Combine(path, Constants.LogSubFolder, "Log.log"),
                            rollingInterval: RollingInterval.Day,
                            fileSizeLimitBytes: 5242880,
                            rollOnFileSizeLimit: true,
                            retainedFileCountLimit: 3,
                            outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} ({SourceContext}) {Exception}{NewLine}");

                        l.Filter.ByExcluding(e => e.Properties.ContainsKey(TimeSyncServicePropertyName));
                    })
                    .WriteTo.Logger(l =>
                    {
                        l.WriteTo.File(Path.Combine(path, Constants.LogSubFolder, "TimeSyncService.log"),
                            rollingInterval: RollingInterval.Day,
                            fileSizeLimitBytes: 16384,
                            rollOnFileSizeLimit: true,
                            retainedFileCountLimit: 1,
                            outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} ({SourceContext}) {Exception}{NewLine}");

                        l.Filter.ByIncludingOnly(e => e.Properties.ContainsKey(TimeSyncServicePropertyName));
                    });


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
            serviceCollection.AddScoped<IConfiguration>(_ => configurationRoot);
            serviceCollection.AddSingleton<ILocationService, LocationService>();
            serviceCollection.AddViewModels<DownloadPageViewModel>();
            serviceCollection.AddSingleton<MapPageViewModel>();
            serviceCollection.AddViewModels<NodesViewModel>();
            serviceCollection.AddViewModels<AddressPanelViewModel>();
            serviceCollection.AddSingleton<ConfigPageViewModel>();

            serviceCollection.AddSingleton<ITimeInfoValidator, TimeInfoValidator>();

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
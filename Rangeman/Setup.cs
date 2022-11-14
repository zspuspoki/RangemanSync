using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Serilog;
using System;
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
            //serviceCollection.AddView<AboutPage, AboutViewModel>()
            //    .AddView<ItemsPage, ItemsViewModel>()
            //    .AddView<ItemDetailPage>()
            //    .AddView<NewItemPage>();

            serviceCollection.AddView<ConfigPage, ConfigPageViewModel>();
            serviceCollection.AddView<MainPage, MainPageViewModel>();
            serviceCollection.AddView<MapPage, MapPageViewModel>();
            return serviceCollection;
        }

        public static IServiceCollection ConfigureLogging(this IServiceCollection serviceCollection, IConfigurationRoot configurationRoot)
        {
            return serviceCollection.AddLogging(builder =>
            {
                builder.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(configurationRoot.GetSection("Logging"))
                    .CreateLogger());
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
            //serviceCollection.AddTransient(typeof(MyRouteFactory<>))
            //    .AddTransient<IDataStore<Item>, MockDataStore>()
            //    .AddViews()
            //    .AddViewModels<BaseViewModel>();
            serviceCollection.AddViewModels<MainPageViewModel>();
            serviceCollection.AddSingleton<MapPageViewModel>();
            serviceCollection.AddTransient<ConfigPageViewModel>((ctx) =>
            {
                var mapPageViewModel = ctx.GetService<MapPageViewModel>();
                var configPageViewModel = new ConfigPageViewModel();
                configPageViewModel.UseMbTilesClicked += (s, e) => mapPageViewModel.UpdateMapToUseMbTilesFile();
                return configPageViewModel;
            });

            serviceCollection.AddViews();

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

        private static IServiceCollection AddView<TView, TViewModel>(this IServiceCollection serviceCollection) where TView : Page
        {
            return serviceCollection.AddTransient<TView>(serviceProvider =>
            {
                TView view = ActivatorUtilities.CreateInstance<TView>(serviceProvider);

                // Autobind specified view model
                view.BindingContext = serviceProvider.GetRequiredService<TViewModel>();

                view.Appearing += (sender, args) => (((BindableObject)sender).BindingContext as IPageLifeCycleAware)?.OnAppearing();
                view.Disappearing += (sender, args) => (((BindableObject)sender).BindingContext as IPageLifeCycleAware)?.OnDisappearing();

                return view;
            });
        }

        private static IServiceCollection AddView<TView>(this IServiceCollection serviceCollection) where TView : Page
        {
            return serviceCollection.AddTransient<TView>(serviceProvider =>
            {
                TView view = ActivatorUtilities.CreateInstance<TView>(serviceProvider);

                view.Appearing += (sender, args) => (((BindableObject)sender).BindingContext as IPageLifeCycleAware)?.OnAppearing();
                view.Disappearing += (sender, args) => (((BindableObject)sender).BindingContext as IPageLifeCycleAware)?.OnDisappearing();

                return view;
            });
        }

    }
}
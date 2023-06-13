using Microsoft.Extensions.DependencyInjection;
using Rangeman.Views.Coordinates;
using Rangeman.Views.Tide;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rangeman
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell : Shell
    {
        public AppShell(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;

            Routing.RegisterRoute("Download", ServiceProvider.GetRequiredService<MyRouteFactory<DownloadPage>>());
            Routing.RegisterRoute("Map", ServiceProvider.GetRequiredService<MyRouteFactory<MapPage>>());
            Routing.RegisterRoute("Coordinates", ServiceProvider.GetRequiredService<MyRouteFactory<CoordinatesPage>>());
            Routing.RegisterRoute("Tide", ServiceProvider.GetRequiredService<MyRouteFactory<Tide>>());
            Routing.RegisterRoute("Config", ServiceProvider.GetRequiredService<MyRouteFactory<ConfigPage>>());

            InitializeComponent();
            BindingContext = ServiceProvider.GetRequiredService<AppShellViewModel>();
        }

        public IServiceProvider ServiceProvider { get; }
    }
}
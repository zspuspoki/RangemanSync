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

            AddRouteIfNotExist("Download", ServiceProvider.GetRequiredService<MyRouteFactory<DownloadPage>>());
            AddRouteIfNotExist("Map", ServiceProvider.GetRequiredService<MyRouteFactory<MapPage>>());
            AddRouteIfNotExist("Coordinates", ServiceProvider.GetRequiredService<MyRouteFactory<CoordinatesPage>>());
            AddRouteIfNotExist("Tide", ServiceProvider.GetRequiredService<MyRouteFactory<Tide>>());
            AddRouteIfNotExist("Config", ServiceProvider.GetRequiredService<MyRouteFactory<ConfigPage>>());

            InitializeComponent();
            BindingContext = ServiceProvider.GetRequiredService<AppShellViewModel>();
        }

        private void AddRouteIfNotExist(string route, RouteFactory factory)
        {
            if(Routing.GetOrCreateContent(route) == null)
            {
                Routing.RegisterRoute(route, factory);
            }
        }

        public IServiceProvider ServiceProvider { get; }
    }
}
using Microsoft.Extensions.DependencyInjection;
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

            Routing.RegisterRoute("Download", ServiceProvider.GetRequiredService<MyRouteFactory<MainPage>>());
            Routing.RegisterRoute("Map", ServiceProvider.GetRequiredService<MyRouteFactory<MapPage>>());
            Routing.RegisterRoute("Config", ServiceProvider.GetRequiredService<MyRouteFactory<ConfigPage>>());

            InitializeComponent();
            BindingContext = ServiceProvider.GetRequiredService<AppShellViewModel>();
        }

        public IServiceProvider ServiceProvider { get; }
    }
}
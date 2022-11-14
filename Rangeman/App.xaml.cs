using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rangeman
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App : Application
    {
        public App(Action<ConfigurationBuilder> configuration, 
            Action<IServiceCollection, IConfigurationRoot> dependencyServiceConfiguration)
        {
            InitializeComponent();

            IConfigurationRoot configurationRoot = Setup.Configuration
                .ConfigureRangemanProject()
                .ConfigureRangemanAppProject(configuration)
                .Build();

            IServiceProvider serviceProvider = Setup.DependencyInjection
                .ConfigureLogging(configurationRoot)
                .ConfigureRangemanProject(configurationRoot)
                .ConfigureRangemanAppProject(configurationRoot, dependencyServiceConfiguration)
                .BuildServiceProvider();

            MainPage = new AppShell(serviceProvider);
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

    }
}
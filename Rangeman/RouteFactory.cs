using Microsoft.Extensions.DependencyInjection;
using System;
using Xamarin.Forms;

namespace Rangeman
{
    public class MyRouteFactory<T> : RouteFactory where T : Element
    {
        public MyRouteFactory(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        protected IServiceProvider ServiceProvider { get; }

        public override Element GetOrCreate() => ServiceProvider.GetRequiredService<T>();
    }
}
using System;
using Xamarin.Forms;

namespace Rangeman
{
    public static class ShellExtension
    {
        public static IServiceProvider ServiceProvider(this Shell shell)
        {
            return (shell as AppShell)?.ServiceProvider;
        }
    }
}
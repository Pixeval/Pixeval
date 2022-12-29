using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace Pixeval.Startup.WinUI.Hosting
{
    public static class ServiceCollectionExtensions
    {
        [DllImport("Microsoft.ui.xaml.dll")]
        private static extern void XamlCheckProcessRequirements();

        public static IServiceCollection AddWinUIApp<T>(this IServiceCollection services) where T : Application
        {
            services.AddSingleton<Application, T>(provider =>
            {
                XamlCheckProcessRequirements();
                WinRT.ComWrappersSupport.InitializeComWrappers();
                return (Activator.CreateInstance(typeof(T), provider) as T)!;
            });
            return services.AddHostedService<WinUIAppStartupService>();
        }
    }
}

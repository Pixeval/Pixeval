using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pixeval.Wpf.View;

namespace Pixeval.Wpf
{
    class Program
    {
        [STAThread]
        private static void Main()=> CreateHostBuilder().Build().Run();

        private static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<SignIn>();
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<App>();
                    services.AddHostedService<Worker>();
                });
    }
}

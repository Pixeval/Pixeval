using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pixeval.UI;

namespace Pixeval
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

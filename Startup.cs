using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.UI;

namespace Pixeval
{
    class Startup
    {
        public static void  ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddSingleton<SignIn>();
        }
    }
}

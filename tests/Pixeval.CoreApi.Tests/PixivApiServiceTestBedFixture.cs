using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Pixeval.CoreApi.Tests
{
    public class PixivApiServiceTestBedFixture : TestBedFixture
    {
        protected override void AddServices(IServiceCollection services, IConfiguration? configuration)
        {
            services.AddLogging(config =>
            {
                config.AddDebug();
            });

            services.Configure<PixivClientOptions>(options =>
            {
                options.Bypass = true;
                options.ConnectionTimeout = 5000;
                options.CultureInfo = CultureInfo.CurrentCulture;
            });

            services.Configure<PixivHttpOptions>(configuration.GetSection("PixivHttpOptions"));
            services.AddSingleton<ISessionRefresher, SessionRefresher>();
            services.AddSingleton(new SessionStorage(configuration["RefreshToken"]!));
            services.AddPixivApiService(httpClient =>
            {

            });
        }

        protected override IEnumerable<TestAppSettings> GetTestAppSettings()
        {
            yield return new TestAppSettings() { Filename = "appsettings.json", IsOptional = false };
            yield return new TestAppSettings() { Filename = "secrets.json", IsOptional = false };
        }

        protected override ValueTask DisposeAsyncCore()
        {
            return new();
        }
    }
}

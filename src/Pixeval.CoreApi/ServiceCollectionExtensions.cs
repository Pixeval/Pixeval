using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Pixeval.CoreApi.Models;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.Requests;
using Pixeval.CoreApi.Services;
using Polly;
using Polly.Bulkhead;
using Polly.Extensions.Http;
using Refit;

namespace Pixeval.CoreApi
{
    public static class ServiceCollectionExtensions
    {

        private static void AddNameResolvers(IServiceCollection services)
        {
            services.TryAddTransient<LocalMachineNameResolver>();
            services.TryAddTransient<PixivImageNameResolver>();
            services.TryAddTransient<PixivApiNameResolver>();
        }

        private static void AddMessageHandlers(IServiceCollection services)
        {
            services.TryAddTransient<PixivAppApiHttpClientHandler>();
        }

        public static IServiceCollection AddPixivApiSession<TSessionRefresher, TSessionStorage>(this IServiceCollection services) where TSessionRefresher : class, ISessionRefresher where TSessionStorage : AbstractSessionStorage
        {
            services.AddSingleton<ISessionRefresher, TSessionRefresher>();
            services.AddSingleton<AbstractSessionStorage, TSessionStorage>();
            return services;
        }

        public static IServiceCollection AddPixivApiService(this IServiceCollection services, Action<HttpClient> configuration) 
        {
            AddNameResolvers(services);
            AddMessageHandlers(services);

            services.AddRefitClient<IPixivAppService>()
                .ConfigureHttpClient((provider, httpClient) =>
            {
                var options = provider.GetService<IOptions<PixivHttpOptions>>();
                httpClient.BaseAddress = new(options.Value.AppApiBaseUrl);
                configuration.Invoke(httpClient);
            })
                .AddPolicyHandlerFromRegistry("RetryRefreshToken")
                .ConfigurePrimaryHttpMessageHandler<PixivAppApiHttpClientHandler>();

            services.AddRefitClient<IPixivAuthService>()
                .ConfigureHttpClient((provider, httpClient) =>
            {
                var options = provider.GetService<IOptions<PixivHttpOptions>>();
                httpClient.BaseAddress = new(options.Value.OAuthBaseUrl);
                configuration.Invoke(httpClient);
            });
            services.AddRefitClient<IPixivImageService>()
                .ConfigureHttpClient(httpClient =>
            {
                configuration.Invoke(httpClient);
            });
            services.AddRefitClient<IPixivReverseSearchService>()
                .ConfigureHttpClient(httpClient =>
            {
                configuration.Invoke(httpClient);
            });

            services.AddPolicyRegistry((provider, registry) =>
            {
                var sessionRefresher = provider.GetService<ISessionRefresher>()!;
                var sessionStorage = provider.GetService<AbstractSessionStorage>()!;
                registry.Add("RetryRefreshToken",
                    Policy
                        .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.BadRequest)
                        .RetryAsync(1, onRetryAsync:
                        async (exception, retryCount, context) =>
                        {
                            var session = await sessionStorage.GetSessionAsync();
                            TokenResponse? tokenResponse;
                            if (session is null)
                            {
                                tokenResponse = await sessionRefresher.ExchangeTokenAsync();
                            }
                            else
                            {
                                tokenResponse = await sessionRefresher.RefreshTokenAsync(session.RefreshToken);
                            }
                            await sessionStorage.SetSessionAsync(tokenResponse.User!.Id!, tokenResponse.RefreshToken!,
                                tokenResponse.AccessToken!);
                        }));
            });
            return services;
        }
    }
}

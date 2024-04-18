#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/MakoClient.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Global.Exception;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.EndPoints;
using Pixeval.CoreApi.Net.Request;
using Pixeval.CoreApi.Preference;
using Pixeval.Logging;
using Pixeval.Utilities;
using Refit;

namespace Pixeval.CoreApi;

public partial class MakoClient : ICancellable, IAsyncDisposable
{
    /// <summary>
    /// Create a new <see cref="MakoClient" /> based on given <see cref="Configuration" />, <see cref="Session" />
    /// </summary>
    /// <remarks>
    /// The <see cref="MakoClient" /> is not responsible for the <see cref="Session" />'s refreshment.
    /// </remarks>
    /// <param name="session">The <see cref="Preference.Session" /></param>
    /// <param name="configuration">The <see cref="Configuration" /></param>
    /// <param name="logger"></param>
    public MakoClient(Session session, MakoClientConfiguration configuration, FileLogger logger)
    {
        Logger = logger;
        Session = session;
        MakoServices = BuildServiceProvider(ServiceCollection);
        Configuration = configuration;
        IsCancelled = false;
    }

    public static async Task<MakoClient?> TryGetMakoClientAsync(string refreshToken, MakoClientConfiguration configuration, FileLogger logger)
    {
        var makoClient = new MakoClient(null!, configuration, logger);
        try
        {
            makoClient.Session = (await makoClient.MakoServices.GetRequiredService<IAuthEndPoint>().RefreshAsync(new RefreshSessionRequest(refreshToken)).ConfigureAwait(false)).ToSession();
            return makoClient;
        }
        catch
        {
            await makoClient.DisposeAsync();
            return null;
        }
    }

    /// <summary>
    /// Injects necessary dependencies
    /// </summary>
    /// <returns>The <see cref="ServiceProvider" /> contains all the required dependencies</returns>
    private ServiceProvider BuildServiceProvider(IServiceCollection serviceCollection) =>
        serviceCollection
            .AddSingleton(this)
            .AddSingleton<PixivApiHttpMessageHandler>()
            .AddSingleton<PixivImageHttpMessageHandler>()
            .AddKeyedSingleton<HttpMessageHandler, MakoRetryHttpClientHandler>(typeof(PixivApiHttpMessageHandler),
                (s, _) => new(s.GetRequiredService<MakoClient>(), s.GetRequiredService<PixivApiHttpMessageHandler>()))
            .AddKeyedSingleton<HttpMessageHandler, MakoRetryHttpClientHandler>(typeof(PixivImageHttpMessageHandler),
                (s, _) => new(s.GetRequiredService<MakoClient>(), s.GetRequiredService<PixivImageHttpMessageHandler>()))
            .AddKeyedSingleton<HttpClient, MakoHttpClient>(MakoApiKind.AppApi,
                (s, _) => new(s.GetRequiredKeyedService<HttpMessageHandler>(typeof(PixivApiHttpMessageHandler)))
                {
                    BaseAddress = new Uri(MakoHttpOptions.AppApiBaseUrl)
                })
            .AddKeyedSingleton<HttpClient, MakoHttpClient>(MakoApiKind.WebApi,
                (s, _) => new(s.GetRequiredKeyedService<HttpMessageHandler>(typeof(PixivApiHttpMessageHandler)))
                {
                    BaseAddress = new Uri(MakoHttpOptions.WebApiBaseUrl)
                })
            .AddKeyedSingleton<HttpClient, MakoHttpClient>(MakoApiKind.AuthApi,
                (s, _) => new(s.GetRequiredKeyedService<HttpMessageHandler>(typeof(PixivApiHttpMessageHandler)))
                {
                    BaseAddress = new Uri(MakoHttpOptions.OAuthBaseUrl)
                })
            .AddKeyedSingleton<HttpClient, MakoHttpClient>(MakoApiKind.ImageApi,
                (s, _) => new(s.GetRequiredKeyedService<HttpMessageHandler>(typeof(PixivImageHttpMessageHandler)))
                {
                    DefaultRequestHeaders =
                    {
                        Referrer = new Uri("https://www.pixiv.net"),
                        UserAgent = { new("PixivIOSApp", "5.8.7") }
                    }
                })
            .AddSingleton(s => RestService.For<IAppApiEndPoint>(
                s.GetRequiredKeyedService<HttpClient>(MakoApiKind.AppApi),
                new RefitSettings
                {
                    ExceptionFactory = async message =>
                        !message.IsSuccessStatusCode
                            ? await MakoNetworkException
                                .FromHttpResponseMessageAsync(message,
                                    s.GetRequiredService<MakoClient>().Configuration.Bypass).ConfigureAwait(false)
                            : null
                }))
            .AddSingleton(s => RestService.For<IAuthEndPoint>(
                s.GetRequiredKeyedService<HttpClient>(MakoApiKind.AuthApi),
                new RefitSettings
                {
                    ExceptionFactory = async message =>
                        !message.IsSuccessStatusCode
                            ? await MakoNetworkException
                                .FromHttpResponseMessageAsync(message,
                                    s.GetRequiredService<MakoClient>().Configuration.Bypass).ConfigureAwait(false)
                            : null
                }))
            .AddSingleton(s => RestService.For<IReverseSearchApiEndPoint>("https://saucenao.com/",
                new RefitSettings
                {
                    ExceptionFactory = async message =>
                        !message.IsSuccessStatusCode
                            ? await MakoNetworkException
                                .FromHttpResponseMessageAsync(message,
                                    s.GetRequiredService<MakoClient>().Configuration.Bypass).ConfigureAwait(false)
                            : null
                }))
            .BuildServiceProvider();

    /// <summary>
    /// Cancels this <see cref="MakoClient" />, including all the running instances, the
    /// <see cref="Session" /> will be reset to its default value, the <see cref="MakoClient" />
    /// will unable to be used again after calling this method
    /// </summary>
    public void Cancel()
    {
        Session = null!;
        _runningInstances.ForEach(instance => instance.EngineHandle.Cancel());
    }

    /// <summary>
    /// Ensures the current instances hasn't been cancelled
    /// </summary>
    /// <exception cref="OperationCanceledException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureNotCancelled()
    {
        if (IsCancelled)
        {
            ThrowUtils.InvalidOperation($"MakoClient({Id}) has been cancelled");
        }
    }

    /// <summary>
    /// registers an instance to the running instances list
    /// </summary>
    /// <param name="engineHandleSource"></param>
    private void RegisterInstance(IEngineHandleSource engineHandleSource)
    {
        _runningInstances.Add(engineHandleSource);
    }

    /// <summary>
    /// removes an instance from the running instances list
    /// </summary>
    /// <param name="handle"></param>
    private void CancelInstance(EngineHandle handle)
    {
        _ = _runningInstances.RemoveAll(instance => instance.EngineHandle == handle);
    }

    /// <summary>
    /// <see cref="PrivacyPolicy.Private"/> is only allowed when the uid is pointing to yourself
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="privacyPolicy"></param>
    /// <returns></returns>
    private void CheckPrivacyPolicy(long uid, PrivacyPolicy privacyPolicy)
    {
        if (privacyPolicy is PrivacyPolicy.Private && Session.Id != uid)
            ThrowUtils.Throw(new IllegalPrivatePolicyException(uid));
    }

    /// <summary>
    /// Gets a registered <see cref="IFetchEngine{E}" /> by its <see cref="EngineHandle" />
    /// </summary>
    /// <param name="handle">The <see cref="EngineHandle" /> of the <see cref="IFetchEngine{E}" /></param>
    /// <typeparam name="T">The type of the results of the <see cref="IFetchEngine{E}" /></typeparam>
    /// <returns>The <see cref="IFetchEngine{E}" /> instance</returns>
    public IFetchEngine<T>? GetByHandle<T>(EngineHandle handle)
    {
        return _runningInstances.FirstOrDefault(h => h.EngineHandle == handle) as IFetchEngine<T>;
    }

    /// <summary>
    /// Acquires a configured <see cref="HttpClient" /> for the network traffics
    /// </summary>
    /// <param name="makoApiKind">The kind of API that is going to be used by the request</param>
    /// <returns>The <see cref="HttpClient" /> corresponding to <paramref name="makoApiKind" /></returns>
    public HttpClient GetMakoHttpClient(MakoApiKind makoApiKind)
    {
        return MakoServices.GetRequiredKeyedService<HttpClient>(makoApiKind);
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        Dispose(ServiceCollection);
        await MakoServices.DisposeAsync();
    }

    private static void Dispose(ServiceCollection collection)
    {
        foreach (var item in collection)
            ((item.IsKeyedService
                    ? item.KeyedImplementationInstance
                    : item.ImplementationInstance)
                as IDisposable)?.Dispose();
    }
}

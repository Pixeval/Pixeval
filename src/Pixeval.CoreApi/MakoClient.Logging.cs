using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.CoreApi.Net.EndPoints;
using Pixeval.Utilities;

namespace Pixeval.CoreApi;

public partial class MakoClient
{
    private Task<T[]> RunWithLoggerAsync<T>(Func<IAppApiEndPoint, Task<T[]>> task)
    {
        return RunWithLoggerAsync(() => task(Provider.GetRequiredService<IAppApiEndPoint>()), () => []);
    }

    private Task<IEnumerable<T>> RunWithLoggerAsync<T>(Func<IAppApiEndPoint, Task<IEnumerable<T>>> task)
    {
        return RunWithLoggerAsync(() => task(Provider.GetRequiredService<IAppApiEndPoint>()), () => []);
    }

    private Task<HttpResponseMessage> RunWithLoggerAsync(Func<IAppApiEndPoint, Task<HttpResponseMessage>> task)
    {
        return RunWithLoggerAsync(() => task(Provider.GetRequiredService<IAppApiEndPoint>()), () => new(HttpStatusCode.RequestTimeout));
    }

    private async Task RunWithLoggerAsync(Func<IAppApiEndPoint, Task> task)
    {
        try
        {
            EnsureNotCancelled();
            await task(Provider.GetRequiredService<IAppApiEndPoint>());
        }
        catch (Exception e)
        {
            LogException(e);
        }
    }

    private Task<T> RunWithLoggerAsync<T>(Func<IAppApiEndPoint, Task<T>> task) where T : IDefaultFactory<T>
    {
        return RunWithLoggerAsync(() => task(Provider.GetRequiredService<IAppApiEndPoint>()), T.CreateDefault);
    }

    private Task<T> RunWithLoggerAsync<T>(Func<Task<T>> task) where T : IDefaultFactory<T>
    {
        return RunWithLoggerAsync(task, T.CreateDefault);
    }

    private Task<HttpResponseMessage> RunWithLoggerAsync(Func<Task<HttpResponseMessage>> task)
    {
        return RunWithLoggerAsync(task, () => new(HttpStatusCode.RequestTimeout));
    }

    private Task<T> RunWithLoggerAsync<T>(Func<Task<Result<T>>> task, Func<T> createDefault)
    {
        return RunWithLoggerAsync(async () =>
        {
            var result = await task();
            switch (result)
            {
                case Result<T>.Success { Value: var value }:
                    return value;
                case Result<T>.Failure { Cause: { } e }:
                    LogException(e);
                    return createDefault();
                default:
                    return ThrowUtils.ArgumentOutOfRange<Result<T>, T>(result);
            }
        }, createDefault);
    }

    private async Task<T> RunWithLoggerAsync<T>(Func<Task<T>> task, Func<T> createDefault)
    {
        try
        {
            EnsureNotCancelled();

            return await task();
        }
        catch (Exception e)
        {
            LogException(e);
            return createDefault();
        }
    }

    private Task<T> RunWithLoggerAsync<T>(Func<Task<Result<T>>> task) where T : IDefaultFactory<T>
    {
        return RunWithLoggerAsync(task, T.CreateDefault);
    }

    internal void LogException(Exception e) => Logger.LogError("MakoClient Exception", e);

    [DynamicDependency("ConstructSystemProxy", "SystemProxyInfo", "System.Net.Http")]
    static MakoClient()
    {
        var type = typeof(HttpClient).Assembly.GetType("System.Net.Http.SystemProxyInfo");
        var method = type?.GetMethod("ConstructSystemProxy");
        var @delegate = method?.CreateDelegate<Func<IWebProxy>>();

        _GetCurrentSystemProxy = @delegate ?? ThrowUtils.Throw<MissingMethodException, Func<IWebProxy>>(new("Unable to find proxy functions"));
        HttpClient.DefaultProxy = _GetCurrentSystemProxy();
    }

    private static readonly Func<IWebProxy> _GetCurrentSystemProxy;

    public IWebProxy? CurrentSystemProxy
    {
        get
        {
            switch (Configuration.Proxy)
            {
                case null:
                    return null;
                case "":
                {
                    var now = DateTime.Now;
                    if (now < CoolDown)
                        return HttpClient.DefaultProxy;
                    CoolDown = now.AddSeconds(2);
                    return HttpClient.DefaultProxy = _GetCurrentSystemProxy();
                }
                default:
                    return new WebProxy(Configuration.Proxy);
            }
        }
    }

    private static DateTime CoolDown { get; set; } = DateTime.Now.AddSeconds(2);
}

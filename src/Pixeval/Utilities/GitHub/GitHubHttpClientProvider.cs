// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Mako.Net;
using Misaki;
using Pixeval.AppManagement;
using Pixeval.Models.Options;

namespace Pixeval.Utilities.GitHub;

public sealed class GitHubHttpClientProvider(NetworkSettingsGroup networkSettings) : IDownloadHttpClientService, IDisposable
{
    public const string PlatformKey = "github";

    private static readonly Uri[] _ProxyProbeUris =
    [
        new($"https://{GitHubHttpOptions.Host}"),
        new($"https://{GitHubHttpOptions.ApiHost}"),
        new($"https://{GitHubHttpOptions.AvatarHost}")
    ];

    private readonly Lock _gate = new();
    private readonly Dictionary<string, HttpClient> _clients = [];

    public string Platform => PlatformKey;

    public HttpClient GetApiClient() => GetClient();

    public HttpClient GetImageDownloadClient() => GetClient();

    private HttpClient GetClient()
    {
        var cacheKey = GetCacheKey();
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_clients.TryGetValue(cacheKey, out var client))
                return client;

            client = GitHubDirectHttpClientFactory.Create(networkSettings);
            _clients.Add(cacheKey, client);
            return client;
        }
    }

    private string GetCacheKey()
    {
        var domainFronting = networkSettings.EnableGitHubDomainFronting ? "domain-fronting" : "direct";
        var proxy = networkSettings.ProxyType switch
        {
            ProxyType.None => "proxy:disabled",
            ProxyType.Custom => $"proxy:explicit:{MakoHelper.NormalizeProxyUri(networkSettings.Proxy) ?? ""}",
            ProxyType.System => $"proxy:system:{string.Join("|", _ProxyProbeUris.Select(GetSystemProxyCacheKeyPart))}",
            _ => throw new ArgumentOutOfRangeException(nameof(networkSettings.ProxyType))
        };
        return $"{domainFronting};{proxy}";
    }

    private static string GetSystemProxyCacheKeyPart(Uri uri)
    {
        var proxy = SystemProxyProvider.GetCurrent();
        try
        {
            return $"{uri.AbsoluteUri}:{proxy.IsBypassed(uri)}:{proxy.GetProxy(uri)?.AbsoluteUri}";
        }
        catch (Exception e)
        {
            return $"{uri.AbsoluteUri}:{e.GetType().FullName}";
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
                return;

            _disposed = true;

            foreach (var client in _clients.Values)
                client.Dispose();

            _clients.Clear();
        }
    }

    private bool _disposed;
}

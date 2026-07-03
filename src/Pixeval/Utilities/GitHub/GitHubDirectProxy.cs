// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Net;
using Pixeval.AppManagement;
using Pixeval.Models.Options;

namespace Pixeval.Utilities.GitHub;

internal sealed class GitHubDirectProxy(NetworkSettingsGroup settings) : IWebProxy
{
    public ICredentials? Credentials { get; set; }

    public static IWebProxy Create(NetworkSettingsGroup settings) => new GitHubDirectProxy(settings);

    public Uri? GetProxy(Uri destination)
    {
        if (IsBypassed(destination))
            return destination;

        return settings.ProxyType switch
        {
            ProxyType.System => WebRequest.DefaultWebProxy?.GetProxy(destination) ?? destination,
            ProxyType.Http or ProxyType.Socks4 or ProxyType.Socks4A or ProxyType.Socks5
                => CreateExplicitProxy()?.GetProxy(destination) ?? destination,
            _ => destination
        };
    }

    public bool IsBypassed(Uri host)
    {
        if (settings is { EnablePixivDomainFronting: true, EnableGitHubDirectConnection: true } &&
            GitHubHttpOptions.HasConfiguredResolver(settings, host.Host))
        {
            return true;
        }

        return settings.ProxyType switch
        {
            ProxyType.None => true,
            ProxyType.System => WebRequest.DefaultWebProxy?.IsBypassed(host) ?? true,
            ProxyType.Http or ProxyType.Socks4 or ProxyType.Socks4A or ProxyType.Socks5
                => CreateExplicitProxy() is not { } proxy || proxy.IsBypassed(host),
            _ => true
        };
    }

    private WebProxy? CreateExplicitProxy()
    {
        if (!Uri.TryCreate(settings.Proxy, UriKind.Absolute, out var uri))
            return null;

        var scheme = settings.ProxyType switch
        {
            ProxyType.Http => "http",
            ProxyType.Socks4 => "socks4",
            ProxyType.Socks4A => "socks4a",
            ProxyType.Socks5 => "socks5",
            _ => null
        };

        if (scheme is null)
            return null;

        var builder = new UriBuilder(uri)
        {
            Scheme = scheme
        };
        var proxy = new WebProxy(builder.Uri)
        {
            Credentials = Credentials
        };
        return proxy;
    }
}

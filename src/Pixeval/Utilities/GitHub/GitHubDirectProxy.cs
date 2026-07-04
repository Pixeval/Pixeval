// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Net;
using Mako.Net;
using Pixeval.AppManagement;
using Pixeval.Models.Options;

namespace Pixeval.Utilities.GitHub;

internal sealed class GitHubDirectProxy(NetworkSettingsGroup settings) : IWebProxy
{
    public ICredentials? Credentials { get; set; }

    public static IWebProxy Create(NetworkSettingsGroup settings) => new GitHubDirectProxy(settings);

    public Uri GetProxy(Uri destination)
    {
        if (IsBypassed(destination))
            return destination;

        return settings.ProxyType switch
        {
            ProxyType.System => SystemProxyProvider.GetCurrent().GetProxy(destination) ?? destination,
            ProxyType.Custom => CreateExplicitProxy()?.GetProxy(destination) ?? destination,
            _ => destination
        };
    }

    public bool IsBypassed(Uri host)
    {
        if (settings.EnableGitHubDomainFronting &&
            GitHubHttpOptions.HasConfiguredResolver(settings, host.Host))
        {
            return true;
        }

        return settings.ProxyType switch
        {
            ProxyType.None => true,
            ProxyType.System => SystemProxyProvider.GetCurrent().IsBypassed(host),
            ProxyType.Custom => CreateExplicitProxy() is not { } proxy || proxy.IsBypassed(host),
            _ => true
        };
    }

    private WebProxy? CreateExplicitProxy()
    {
        if (MakoHelper.NormalizeProxyUri(settings.Proxy) is not { } proxyUri ||
            !Uri.TryCreate(proxyUri, UriKind.Absolute, out var uri))
            return null;

        if (settings.ProxyType is not ProxyType.Custom)
            return null;

        var proxy = new WebProxy(uri)
        {
            Credentials = Credentials
        };
        return proxy;
    }
}

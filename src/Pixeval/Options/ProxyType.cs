// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Options;

[LocalizationMetadata(typeof(ProxyTypeResources))]
public enum ProxyType
{
    [LocalizedResource(typeof(MiscResources), nameof(ProxyTypeResources.ProxyOptionSystem))]
    System,

    [LocalizedResource(typeof(MiscResources), nameof(ProxyTypeResources.ProxyOptionNone))]
    None,

    [LocalizedResource(typeof(MiscResources), nameof(ProxyTypeResources.ProxyOptionHttp))]
    Http,

    [LocalizedResource(typeof(MiscResources), nameof(ProxyTypeResources.ProxyOptionSocks4))]
    Socks4,

    [LocalizedResource(typeof(MiscResources), nameof(ProxyTypeResources.ProxyOptionSocks4A))]
    Socks4A,

    [LocalizedResource(typeof(MiscResources), nameof(ProxyTypeResources.ProxyOptionSocks5))]
    Socks5
}

public static class ProxyTypeResources
{
    public static string GetResource(string id) => id switch
    {
        nameof(ProxyOptionHttp) => ProxyOptionHttp,
        nameof(ProxyOptionSocks4) => ProxyOptionSocks4,
        nameof(ProxyOptionSocks4A) => ProxyOptionSocks4A,
        nameof(ProxyOptionSocks5) => ProxyOptionSocks5,
        nameof(ProxyOptionSystem) => ProxyOptionSystem,
        _ => ProxyOptionNone
    };

    public const string ProxyOptionHttp = "http/https";
    public const string ProxyOptionSocks4 = "socks4";
    public const string ProxyOptionSocks4A = "socks4a";
    public const string ProxyOptionSocks5 = "socks5";
    public static string ProxyOptionNone => MiscResources.ProxyOptionNone;
    public static string ProxyOptionSystem => MiscResources.ProxyOptionSystem;
}

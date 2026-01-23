// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Pixeval.Controls;
using Pixeval.I18N;

namespace Pixeval.Models.Options;

public enum ProxyType
{
    System,
    None,
    Http,
    Socks4,
    Socks4A,
    Socks5
}

public static class ProxyTypeExtension
{
    public const string ProxyOptionHttp = "http/https";
    public const string ProxyOptionSocks4 = "socks4";
    public const string ProxyOptionSocks4A = "socks4a";
    public const string ProxyOptionSocks5 = "socks5";

    public static IReadOnlyList<SymbolComboBoxItem> Items =>
    [
        new(ProxyType.System, I18NManager.GetResource(MiscResources.ProxyOptionSystem), default),
        new(ProxyType.None, I18NManager.GetResource(MiscResources.ProxyOptionNone), default),
        new(ProxyType.Http, ProxyOptionHttp, default),
        new(ProxyType.Socks4, ProxyOptionSocks4, default),
        new(ProxyType.Socks4A, ProxyOptionSocks4A, default),
        new(ProxyType.Socks5, ProxyOptionSocks5, default)
    ];
}

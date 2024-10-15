#region Copyright
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/ProxyType.cs
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

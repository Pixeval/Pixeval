// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum ProxyType
{
    [LocalizedResource(EnumResources.ProxyOptionSystem)]
    System,

    [LocalizedResource(EnumResources.ProxyOptionNone)]
    None,

    [LocalizedResource(Resource = "http/https")]
    Http,

    [LocalizedResource(Resource = "socks4")]
    Socks4,

    [LocalizedResource(Resource = "socks4a")]
    Socks4A,

    [LocalizedResource(Resource = "socks5")]
    Socks5
}

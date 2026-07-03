// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum ProxyType
{
    [LocalizedResource(EnumResources.ProxyOptionSystem)]
    System,

    [LocalizedResource(EnumResources.ProxyOptionNone)]
    None,

    [LocalizedResource(EnumResources.ProxyOptionCustom)]
    Custom
}

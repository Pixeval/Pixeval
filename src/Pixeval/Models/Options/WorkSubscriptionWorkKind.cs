// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum WorkSubscriptionWorkKind
{
    [LocalizedResource(EnumResources.WorkSubscriptionWorkKind.Illustration)]
    Illustration,

    [LocalizedResource(EnumResources.WorkSubscriptionWorkKind.Manga)]
    Manga,

    [LocalizedResource(EnumResources.WorkSubscriptionWorkKind.Novel)]
    Novel
}

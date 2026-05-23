// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum WorkSubscriptionWorkKind
{
    [LocalizedResource(EnumResources.WorkSubscriptionWorkKindIllustrationAndManga)]
    IllustrationAndManga,

    [LocalizedResource(EnumResources.WorkSubscriptionWorkKindIllustration)]
    Illustration,

    [LocalizedResource(EnumResources.WorkSubscriptionWorkKindManga)]
    Manga,

    [LocalizedResource(EnumResources.WorkSubscriptionWorkKindNovel)]
    Novel
}

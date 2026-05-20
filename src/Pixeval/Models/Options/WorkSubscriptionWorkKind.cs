// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum WorkSubscriptionWorkKind
{
    [LocalizedResource(EnumResources.WorkSubscriptionWorkKindNovel)]
    Novel,

    [LocalizedResource(EnumResources.WorkSubscriptionWorkKindIllustration)]
    Illustration,

    [LocalizedResource(EnumResources.WorkSubscriptionWorkKindManga)]
    Manga,

    [LocalizedResource(EnumResources.WorkSubscriptionWorkKindIllustrationAndManga)]
    IllustrationAndManga
}

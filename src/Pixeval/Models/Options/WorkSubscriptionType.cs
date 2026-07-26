// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum WorkSubscriptionType
{
    [LocalizedResource(EnumResources.WorkSubscriptionType.Bookmarks)]
    Bookmarks,

    [LocalizedResource(EnumResources.WorkSubscriptionType.Posts)]
    Posts,

    [LocalizedResource(EnumResources.WorkSubscriptionType.Series)]
    Series
}

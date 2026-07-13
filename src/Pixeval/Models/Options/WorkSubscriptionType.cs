// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum WorkSubscriptionType
{
    [LocalizedResource(EnumResources.WorkSubscriptionTypeBookmarks)]
    Bookmarks,

    [LocalizedResource(EnumResources.WorkSubscriptionTypePosts)]
    Posts,

    [LocalizedResource(EnumResources.WorkSubscriptionTypeSeries)]
    Series
}

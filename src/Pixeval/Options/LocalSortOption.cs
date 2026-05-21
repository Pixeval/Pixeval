// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata(typeof(MiscResources))]
public enum LocalSortOption
{
    [LocalizedResource(nameof(MiscResources.WorkSortOptionDoNotSort))]
    DoNotSort,

    [LocalizedResource(nameof(MiscResources.WorkSortOptionPopularityDescending))]
    PublishDateDescending,

    [LocalizedResource(nameof(MiscResources.WorkSortOptionPublishDateAscending))]
    PublishDateAscending,

    [LocalizedResource(nameof(MiscResources.WorkSortOptionPublishDateDescending))]
    PopularityDescending
}

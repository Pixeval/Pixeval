// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using FluentIcons.Common;
using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum LocalSortOption
{
    [LocalizedResource(Symbol.ArrowSort, EnumResources.LocalSortOptionDoNotSort)]
    DoNotSort,

    [LocalizedResource(Symbol.ArrowTrendingSparkle, EnumResources.LocalSortOptionPopularityDescending)]
    PublishDateDescending,

    [LocalizedResource(Symbol.ArrowSortUpLines, EnumResources.LocalSortOptionPublishDateAscending)]
    PublishDateAscending,

    [LocalizedResource(Symbol.ArrowSortDownLines, EnumResources.LocalSortOptionPublishDateDescending)]
    PopularityDescending
}

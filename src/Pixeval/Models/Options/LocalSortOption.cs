// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using FluentIcons.Common;
using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum LocalSortOption
{
    [LocalizedResource(Symbol.ArrowSort, EnumResources.LocalSortOption.DoNotSort)]
    DoNotSort,

    [LocalizedResource(Symbol.ArrowTrendingSparkle, EnumResources.LocalSortOption.PopularityDescending)]
    PopularityDescending,

    [LocalizedResource(Symbol.ArrowSortDownLines, EnumResources.LocalSortOption.PublishDateDescending)]
    PublishDateDescending,

    [LocalizedResource(Symbol.ArrowSortUpLines, EnumResources.LocalSortOption.PublishDateAscending)]
    PublishDateAscending
}

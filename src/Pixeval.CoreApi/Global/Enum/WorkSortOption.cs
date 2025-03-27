// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.ComponentModel;

namespace Mako.Global.Enum;

public enum WorkSortOption
{
    DoNotSort,

    [Description("popular_desc")]
    PopularityDescending,

    [Description("date_asc")]
    PublishDateAscending,

    [Description("date_desc")]
    PublishDateDescending
}

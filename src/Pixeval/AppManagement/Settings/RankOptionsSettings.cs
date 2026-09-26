// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako.Global.Enum;

namespace Pixeval.AppManagement.Settings;

public record RankOptionsSettings
{
    public RankOption IllustrationRankOption { get; set; }

    public RankOption NovelRankOption { get; set; }
}

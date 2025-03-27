// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.ComponentModel;

namespace Mako.Global.Enum;

public enum RankOption
{
    [Description("day")]
    Day,

    [Description("week")]
    Week,

    /// <summary>
    /// Novel 不支持
    /// </summary>
    [Description("month")]
    Month,

    [Description("day_male")]
    DayMale,

    [Description("day_female")]
    DayFemale,

    /// <summary>
    /// Novel 不支持
    /// </summary>
    [Description("day_manga")]
    DayManga,

    /// <summary>
    /// Novel 不支持
    /// </summary>
    [Description("week_manga")]
    WeekManga,

    /// <summary>
    /// Novel 不支持
    /// </summary>
    [Description("month_manga")]
    MonthManga,

    /// <summary>
    /// Novel 不支持
    /// </summary>
    [Description("week_original")]
    WeekOriginal,

    [Description("week_rookie")]
    WeekRookie,

    [Description("day_r18")]
    DayR18,

    [Description("day_male_r18")]
    DayMaleR18,

    [Description("day_female_r18")]
    DayFemaleR18,

    [Description("week_r18")]
    WeekR18,

    [Description("week_r18g")]
    WeekR18G,

    [Description("day_ai")]
    DayAi,

    [Description("day_r18_ai")]
    DayR18Ai
}

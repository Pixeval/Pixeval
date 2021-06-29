using JetBrains.Annotations;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi
{
    [PublicAPI]
    public enum RankOption
    {
        [Description("day")]
        Day,

        [Description("week")]
        Week,

        [Description("month")]
        Month,

        [Description("day_male")]
        DayMale,

        [Description("day_female")]
        DayFemale,

        [Description("day_manga")]
        DayManga,

        [Description("week_manga")]
        WeekManga,

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
        WeekR18G
    }
}
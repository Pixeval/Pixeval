using System.Collections.Generic;
using Mako.Global.Enum;
using Pixeval.Util;
using Pixeval.Util.Generic;

namespace Pixeval.Options
{
    public record RankOptionWrapper : ILocalizedBox<RankOption, RankOptionWrapper>
    {
        public RankOption Value { get; }

        public string LocalizedString { get; }

        public RankOptionWrapper(RankOption value, string localizedString)
        {
            Value = value;
            LocalizedString = localizedString;
        }

        public static IEnumerable<RankOptionWrapper> AvailableOptions()
        {
            return new RankOptionWrapper[]
            {
                new(RankOption.Day, RankingsPageResources.RankOptionDay),
                new(RankOption.Week, RankingsPageResources.RankOptionWeek),
                new(RankOption.Month, RankingsPageResources.RankOptionMonth),
                new(RankOption.DayMale, RankingsPageResources.RankOptionDayMale),
                new(RankOption.DayFemale, RankingsPageResources.RankOptionDayFemale),
                new(RankOption.DayManga, RankingsPageResources.RankOptionDayManga),
                new(RankOption.WeekManga, RankingsPageResources.RankOptionWeekManga),
                new(RankOption.WeekOriginal, RankingsPageResources.RankOptionWeekOriginal),
                new(RankOption.WeekRookie, RankingsPageResources.RankOptionWeekRookie),
                new(RankOption.DayR18, RankingsPageResources.RankOptionDayR18),
                new(RankOption.DayMaleR18, RankingsPageResources.RankOptionDayMaleR18),
                new(RankOption.DayFemaleR18, RankingsPageResources.RankOptionDayFemaleR18),
                new(RankOption.WeekR18, RankingsPageResources.RankOptionWeekR18),
                new(RankOption.WeekR18G, RankingsPageResources.RankOptionWeekR18G)
            };
        }
    }
}
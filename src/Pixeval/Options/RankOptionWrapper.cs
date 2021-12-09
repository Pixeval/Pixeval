#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/RankOptionWrapper.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Util.Generic;

namespace Pixeval.Options;

public record RankOptionWrapper(RankOption Value, string LocalizedString) : ILocalizedBox<RankOption, RankOptionWrapper>
{
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
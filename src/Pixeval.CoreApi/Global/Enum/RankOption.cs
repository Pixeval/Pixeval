#region Copyright (c) Pixeval/Pixeval.CoreApi

// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/RankOption.cs
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

using JetBrains.Annotations;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Global.Enum
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
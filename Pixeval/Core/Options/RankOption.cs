#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using Pixeval.Objects.Primitive;

namespace Pixeval.Core.Options
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ForR18Only : Attribute
    {
    }

    public enum RankOption
    {
        /// <summary>
        ///     日榜
        /// </summary>
        [EnumAlias("day")]
        [EnumLocalizedName("RankOptionDay")]
        Day,

        /// <summary>
        ///     周榜
        /// </summary>
        [EnumAlias("week")]
        [EnumLocalizedName("RankOptionWeek")]
        Week,

        /// <summary>
        ///     月榜
        /// </summary>
        [EnumAlias("month")]
        [EnumLocalizedName("RankOptionMonth")]
        Month,

        /// <summary>
        ///     男性向日榜
        /// </summary>
        [EnumAlias("day_male")]
        [EnumLocalizedName("RankOptionDayMale")]
        DayMale,

        /// <summary>
        ///     女性向日榜
        /// </summary>
        [EnumAlias("day_female")]
        [EnumLocalizedName("RankOptionDayFemale")]
        DayFemale,

        /// <summary>
        ///     多图日榜
        /// </summary>
        [EnumAlias("day_manga")]
        [EnumLocalizedName("RankOptionDayManga")]
        DayManga,

        /// <summary>
        ///     多图周榜
        /// </summary>
        [EnumAlias("week_manga")]
        [EnumLocalizedName("RankOptionWeekManga")]
        WeekManga,

        /// <summary>
        ///     原创
        /// </summary>
        [EnumAlias("week_original")]
        [EnumLocalizedName("RankOptionWeekOriginal")]
        WeekOriginal,

        /// <summary>
        ///     新人
        /// </summary>
        [EnumAlias("week_rookie")]
        [EnumLocalizedName("RankOptionWeekRookie")]
        WeekRookie,

        /// <summary>
        ///     R18日榜
        /// </summary>
        [ForR18Only]
        [EnumAlias("day_r18")]
        [EnumLocalizedName("RankOptionDayR18")]
        DayR18,

        /// <summary>
        ///     男性向R18日榜
        /// </summary>
        [ForR18Only]
        [EnumAlias("day_male_r18")]
        [EnumLocalizedName("RankOptionDayMaleR18")]
        DayMaleR18,

        /// <summary>
        ///     女性向R18日榜
        /// </summary>
        [ForR18Only]
        [EnumAlias("day_female_r18")]
        [EnumLocalizedName("RankOptionDayFemaleR18")]
        DayFemaleR18,

        /// <summary>
        ///     R18周榜
        /// </summary>
        [ForR18Only]
        [EnumAlias("week_r18")]
        [EnumLocalizedName("RankOptionWeekR18")]
        WeekR18,

        /// <summary>
        ///     R18G周榜
        /// </summary>
        [ForR18Only]
        [EnumAlias("week_r18g")]
        [EnumLocalizedName("RankOptionWeekR18G")]
        WeekR18G
    }
}
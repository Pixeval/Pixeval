#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/RankOption.cs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using JetBrains.Annotations;
using Pixeval.CoreApi.Util;

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
#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/AttributeHelper.cs
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

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.UI.Xaml;
using Pixeval.Attributes;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;
using WinUI3Utilities;

namespace Pixeval.Util;

public static class AttributeHelper
{
    public static TAttribute? GetCustomAttribute<TAttribute>(this Enum e) where TAttribute : Attribute
    {
        return e.GetType().GetField(e.ToString())?.GetCustomAttribute(typeof(TAttribute), false) as TAttribute;
    }
}

public static class LocalizedResourceAttributeHelper
{
    public static List<StringRepresentableItem> GetLocalizedResourceContents<T>() where T : struct, Enum
    {
        return GetLocalizedResourceContents(Enum.GetValues<T>());
    }

    public static List<StringRepresentableItem> GetLocalizedResourceContents<T>(T[] enumType)
    {
        return GetLocalizedResourceContents(enumType2: enumType);
    }

    public static List<StringRepresentableItem> GetLocalizedResourceContents(Array enumType2)
    {
        var items = new List<StringRepresentableItem>();
        foreach (var value in enumType2)
            if (value is Enum t && t.GetLocalizedResourceContent() is { } content)
                items.Add(new StringRepresentableItem(t, content));
        return items;
    }

    public static string? GetLocalizedResourceContent(this Enum e)
    {
        if (_predefinedResources.TryGetValue(e, out var v))
            return v;
        var attribute = e.GetCustomAttribute<LocalizedResource>();
        return attribute?.GetLocalizedResourceContent();
    }

    public static LocalizedResource? GetLocalizedResource(this Enum e)
    {
        return e.GetCustomAttribute<LocalizedResource>();
    }

    public static string? GetLocalizedResourceContent(this LocalizedResource attribute)
    {
        return GetLocalizedResourceContent(attribute.ResourceLoader, attribute.Key);
    }

    public static string? GetLocalizedResourceContent(Type resourceLoader, string key)
    {
        return resourceLoader.GetMember(key, BindingFlags.Static | BindingFlags.Public) switch
        {
        [FieldInfo fi] => fi?.GetValue(null),
        [PropertyInfo pi] => pi?.GetValue(null),
            _ => null
        } as string;
    }

    private static readonly Dictionary<Enum, string> _predefinedResources = new()
    {
        [TargetFilter.ForAndroid] = MiscResources.TargetFilterForAndroid,
        [TargetFilter.ForIos] = MiscResources.TargetFilterForIOS,

        [BackdropType.Acrylic] = MiscResources.AcrylicBackdrop,
        [BackdropType.Mica] = MiscResources.MicaBackdrop,
        [BackdropType.MicaAlt] = MiscResources.MicaAltBackdrop,
        [BackdropType.None] = MiscResources.NoneBackdrop,

        [SearchIllustrationTagMatchOption.PartialMatchForTags] = MiscResources.SearchIllustrationTagMatchOptionPartialMatchForTags,
        [SearchIllustrationTagMatchOption.ExactMatchForTags] = MiscResources.SearchIllustrationTagMatchOptionExactMatchForTags,
        [SearchIllustrationTagMatchOption.TitleAndCaption] = MiscResources.SearchIllustrationTagMatchOptionTitleAndCaption,

        [SearchNovelTagMatchOption.PartialMatchForTags] = MiscResources.SearchNovelTagMatchOptionPartialMatchForTags,
        [SearchNovelTagMatchOption.ExactMatchForTags] = MiscResources.SearchNovelTagMatchOptionExactMatchForTags,
        [SearchNovelTagMatchOption.Text] = MiscResources.SearchNovelTagMatchOptionText,
        [SearchNovelTagMatchOption.Keyword] = MiscResources.SearchNovelTagMatchOptionCaption,

        [ElementTheme.Dark] = MiscResources.AppThemeDark,
        [ElementTheme.Light] = MiscResources.AppThemeLight,
        [ElementTheme.Default] = MiscResources.AppThemeSystemDefault,

        [WorkSortOption.PopularityDescending] = MiscResources.IllustrationSortOptionPopularityDescending,
        [WorkSortOption.PublishDateAscending] = MiscResources.IllustrationSortOptionPublishDateAscending,
        [WorkSortOption.PublishDateDescending] = MiscResources.IllustrationSortOptionPublishDateDescending,
        [WorkSortOption.DoNotSort] = MiscResources.IllustrationSortOptionDoNotSort,

        [SearchDuration.Undecided] = MiscResources.SearchDurationUndecided,
        [SearchDuration.WithinLastDay] = MiscResources.SearchDurationWithinLastDay,
        [SearchDuration.WithinLastWeek] = MiscResources.SearchDurationWithinLastWeek,
        [SearchDuration.WithinLastMonth] = MiscResources.SearchDurationWithinLastMonth,
        [SearchDuration.WithinLastHalfYear] = MiscResources.SearchDurationWithinLastHalfYear,
        [SearchDuration.WithinLastYear] = MiscResources.SearchDurationWithinLastYear,

        [PrivacyPolicy.Public] = MiscResources.PrivacyPolicyPublic,
        [PrivacyPolicy.Private] = MiscResources.PrivacyPolicyPrivate,

        [WorkType.Illust] = MiscResources.WorkTypeIllust,
        [WorkType.Manga] = MiscResources.WorkTypeManga,
        [WorkType.Novel] = MiscResources.WorkTypeNovel,

        [SimpleWorkType.IllustAndManga] = MiscResources.SimpleWorkTypeIllustAndManga,
        [SimpleWorkType.Novel] = MiscResources.SimpleWorkTypeNovel,

        [RankOption.Day] = RankingsPageResources.RankOptionDay,
        [RankOption.Week] = RankingsPageResources.RankOptionWeek,
        [RankOption.Month] = RankingsPageResources.RankOptionMonth,
        [RankOption.DayMale] = RankingsPageResources.RankOptionDayMale,
        [RankOption.DayFemale] = RankingsPageResources.RankOptionDayFemale,
        [RankOption.DayManga] = RankingsPageResources.RankOptionDayManga,
        [RankOption.WeekManga] = RankingsPageResources.RankOptionWeekManga,
        [RankOption.MonthManga] = RankingsPageResources.RankOptionMonthManga,
        [RankOption.WeekOriginal] = RankingsPageResources.RankOptionWeekOriginal,
        [RankOption.WeekRookie] = RankingsPageResources.RankOptionWeekRookie,
        [RankOption.DayR18] = RankingsPageResources.RankOptionDayR18,
        [RankOption.DayMaleR18] = RankingsPageResources.RankOptionDayMaleR18,
        [RankOption.DayFemaleR18] = RankingsPageResources.RankOptionDayFemaleR18,
        [RankOption.WeekR18] = RankingsPageResources.RankOptionWeekR18,
        [RankOption.WeekR18G] = RankingsPageResources.RankOptionWeekR18G,
        [RankOption.DayAi] = RankingsPageResources.RankOptionDayAi,
        [RankOption.DayR18Ai] = RankingsPageResources.RankOptionDayR18Ai
    };
}

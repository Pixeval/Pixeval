#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/RankOptionExtension.cs
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

using Microsoft.UI.Xaml;
using Pixeval.Attributes;
using WinUI3Utilities;

namespace Pixeval.CoreApi.Global.Enum;

[AttachedLocalizationMetadata<RankOption>(typeof(RankingsPageResources))]
[AttachedLocalizedResource(nameof(RankOption.Day), nameof(RankingsPageResources.RankOptionDay))]
[AttachedLocalizedResource(nameof(RankOption.Week), nameof(RankingsPageResources.RankOptionWeek))]
[AttachedLocalizedResource(nameof(RankOption.Month), nameof(RankingsPageResources.RankOptionMonth))]
[AttachedLocalizedResource(nameof(RankOption.DayMale), nameof(RankingsPageResources.RankOptionDayMale))]
[AttachedLocalizedResource(nameof(RankOption.DayFemale), nameof(RankingsPageResources.RankOptionDayFemale))]
[AttachedLocalizedResource(nameof(RankOption.DayManga), nameof(RankingsPageResources.RankOptionDayManga))]
[AttachedLocalizedResource(nameof(RankOption.WeekManga), nameof(RankingsPageResources.RankOptionWeekManga))]
[AttachedLocalizedResource(nameof(RankOption.MonthManga), nameof(RankingsPageResources.RankOptionMonthManga))]
[AttachedLocalizedResource(nameof(RankOption.WeekOriginal), nameof(RankingsPageResources.RankOptionWeekOriginal))]
[AttachedLocalizedResource(nameof(RankOption.WeekRookie), nameof(RankingsPageResources.RankOptionWeekRookie))]
[AttachedLocalizedResource(nameof(RankOption.DayR18), nameof(RankingsPageResources.RankOptionDayR18))]
[AttachedLocalizedResource(nameof(RankOption.DayMaleR18), nameof(RankingsPageResources.RankOptionDayMaleR18))]
[AttachedLocalizedResource(nameof(RankOption.DayFemaleR18), nameof(RankingsPageResources.RankOptionDayFemaleR18))]
[AttachedLocalizedResource(nameof(RankOption.WeekR18), nameof(RankingsPageResources.RankOptionWeekR18))]
[AttachedLocalizedResource(nameof(RankOption.WeekR18G), nameof(RankingsPageResources.RankOptionWeekR18G))]
[AttachedLocalizedResource(nameof(RankOption.DayAi), nameof(RankingsPageResources.RankOptionDayAi))]
[AttachedLocalizedResource(nameof(RankOption.DayR18Ai), nameof(RankingsPageResources.RankOptionDayR18Ai))]
public static partial class RankOptionExtension;

[AttachedLocalizationMetadata<RankOption>(typeof(RankingsPageResources))]
[AttachedLocalizedResource(nameof(RankOption.Day), nameof(RankingsPageResources.RankOptionDay))]
[AttachedLocalizedResource(nameof(RankOption.Week), nameof(RankingsPageResources.RankOptionWeek))]
[AttachedLocalizedResource(nameof(RankOption.DayMale), nameof(RankingsPageResources.RankOptionDayMale))]
[AttachedLocalizedResource(nameof(RankOption.DayFemale), nameof(RankingsPageResources.RankOptionDayFemale))]
[AttachedLocalizedResource(nameof(RankOption.WeekOriginal), nameof(RankingsPageResources.RankOptionWeekOriginal))]
[AttachedLocalizedResource(nameof(RankOption.WeekRookie), nameof(RankingsPageResources.RankOptionWeekRookie))]
[AttachedLocalizedResource(nameof(RankOption.DayR18), nameof(RankingsPageResources.RankOptionDayR18))]
[AttachedLocalizedResource(nameof(RankOption.DayMaleR18), nameof(RankingsPageResources.RankOptionDayMaleR18))]
[AttachedLocalizedResource(nameof(RankOption.DayFemaleR18), nameof(RankingsPageResources.RankOptionDayFemaleR18))]
[AttachedLocalizedResource(nameof(RankOption.WeekR18), nameof(RankingsPageResources.RankOptionWeekR18))]
[AttachedLocalizedResource(nameof(RankOption.WeekR18G), nameof(RankingsPageResources.RankOptionWeekR18G))]
[AttachedLocalizedResource(nameof(RankOption.DayAi), nameof(RankingsPageResources.RankOptionDayAi))]
[AttachedLocalizedResource(nameof(RankOption.DayR18Ai), nameof(RankingsPageResources.RankOptionDayR18Ai))]
public static partial class NovelRankOptionExtension
{
    public static readonly RankOption[] NovelRankOptions =
    [
        RankOption.Day,
        RankOption.Week,
        // RankOption.Month,
        RankOption.DayMale,
        RankOption.DayFemale,
        // RankOption.DayManga,
        // RankOption.WeekManga,
        // RankOption.MonthManga,
        RankOption.WeekOriginal,
        RankOption.WeekRookie,
        RankOption.DayR18,
        RankOption.DayMaleR18,
        RankOption.DayFemaleR18,
        RankOption.WeekR18,
        RankOption.WeekR18G,
        RankOption.DayAi,
        RankOption.DayR18Ai
    ];
}

[AttachedLocalizationMetadata<TargetFilter>(typeof(MiscResources))]
[AttachedLocalizedResource(nameof(TargetFilter.ForAndroid), nameof(MiscResources.TargetFilterForAndroid))]
[AttachedLocalizedResource(nameof(TargetFilter.ForIos), nameof(MiscResources.TargetFilterForIOS))]
public static partial class TargetFilterExtension;

[AttachedLocalizationMetadata<BackdropType>(typeof(MiscResources))]
[AttachedLocalizedResource(nameof(BackdropType.Acrylic), nameof(MiscResources.AcrylicBackdrop))]
[AttachedLocalizedResource(nameof(BackdropType.Mica), nameof(MiscResources.MicaBackdrop))]
[AttachedLocalizedResource(nameof(BackdropType.MicaAlt), nameof(MiscResources.MicaAltBackdrop))]
[AttachedLocalizedResource(nameof(BackdropType.None), nameof(MiscResources.NoneBackdrop))]
public static partial class BackdropTypeExtension;

[AttachedLocalizationMetadata<SearchIllustrationTagMatchOption>(typeof(MiscResources))]
[AttachedLocalizedResource(nameof(SearchIllustrationTagMatchOption.PartialMatchForTags), nameof(MiscResources.SearchIllustrationTagMatchOptionPartialMatchForTags))]
[AttachedLocalizedResource(nameof(SearchIllustrationTagMatchOption.ExactMatchForTags), nameof(MiscResources.SearchIllustrationTagMatchOptionExactMatchForTags))]
[AttachedLocalizedResource(nameof(SearchIllustrationTagMatchOption.TitleAndCaption), nameof(MiscResources.SearchIllustrationTagMatchOptionTitleAndCaption))]
public static partial class SearchIllustrationTagMatchOptionExtension;

[AttachedLocalizationMetadata<SearchNovelTagMatchOption>(typeof(MiscResources))]
[AttachedLocalizedResource(nameof(SearchNovelTagMatchOption.PartialMatchForTags), nameof(MiscResources.SearchNovelTagMatchOptionPartialMatchForTags))]
[AttachedLocalizedResource(nameof(SearchNovelTagMatchOption.ExactMatchForTags), nameof(MiscResources.SearchNovelTagMatchOptionExactMatchForTags))]
[AttachedLocalizedResource(nameof(SearchNovelTagMatchOption.Text), nameof(MiscResources.SearchNovelTagMatchOptionText))]
[AttachedLocalizedResource(nameof(SearchNovelTagMatchOption.Keyword), nameof(MiscResources.SearchNovelTagMatchOptionCaption))]
public static partial class SearchNovelTagMatchOptionExtension;

[AttachedLocalizationMetadata<ElementTheme>(typeof(MiscResources))]
[AttachedLocalizedResource(nameof(ElementTheme.Dark), nameof(MiscResources.AppThemeDark))]
[AttachedLocalizedResource(nameof(ElementTheme.Light), nameof(MiscResources.AppThemeLight))]
[AttachedLocalizedResource(nameof(ElementTheme.Default), nameof(MiscResources.AppThemeSystemDefault))]
public static partial class ElementThemeExtension;

[AttachedLocalizationMetadata<WorkSortOption>(typeof(MiscResources))]
[AttachedLocalizedResource(nameof(WorkSortOption.PopularityDescending), nameof(MiscResources.WorkSortOptionPopularityDescending))]
[AttachedLocalizedResource(nameof(WorkSortOption.PublishDateAscending), nameof(MiscResources.WorkSortOptionPublishDateAscending))]
[AttachedLocalizedResource(nameof(WorkSortOption.PublishDateDescending), nameof(MiscResources.WorkSortOptionPublishDateDescending))]
[AttachedLocalizedResource(nameof(WorkSortOption.DoNotSort), nameof(MiscResources.WorkSortOptionDoNotSort))]
public static partial class WorkSortOptionExtension;

[AttachedLocalizationMetadata<PrivacyPolicy>(typeof(MiscResources))]
[AttachedLocalizedResource(nameof(PrivacyPolicy.Public), nameof(MiscResources.PrivacyPolicyPublic))]
[AttachedLocalizedResource(nameof(PrivacyPolicy.Private), nameof(MiscResources.PrivacyPolicyPrivate))]
public static partial class PrivacyPolicyExtension;

[AttachedLocalizationMetadata<WorkType>(typeof(MiscResources))]
[AttachedLocalizedResource(nameof(WorkType.Illust), nameof(MiscResources.WorkTypeIllust))]
[AttachedLocalizedResource(nameof(WorkType.Manga), nameof(MiscResources.WorkTypeManga))]
[AttachedLocalizedResource(nameof(WorkType.Novel), nameof(MiscResources.WorkTypeNovel))]
public static partial class WorkTypeExtension;

[AttachedLocalizationMetadata<SimpleWorkType>(typeof(MiscResources))]
[AttachedLocalizedResource(nameof(SimpleWorkType.IllustAndManga), nameof(MiscResources.SimpleWorkTypeIllustAndManga))]
[AttachedLocalizedResource(nameof(SimpleWorkType.Novel), nameof(MiscResources.SimpleWorkTypeNovel))]
public static partial class SimpleWorkTypeExtension;

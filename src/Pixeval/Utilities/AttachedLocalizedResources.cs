// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Avalonia.Media;
using FluentIcons.Common;
using Mako.Global.Enum;
using Pixeval.Attributes;

namespace Pixeval.Utilities;

[AttachedLocalizationMetadata<RankOption>("Illustration")]
[AttachedLocalizedResource(nameof(RankOption.Day), RankingsPageResources.RankOptionDay)]
[AttachedLocalizedResource(nameof(RankOption.Week), RankingsPageResources.RankOptionWeek)]
[AttachedLocalizedResource(nameof(RankOption.Month), RankingsPageResources.RankOptionMonth)]
[AttachedLocalizedResource(nameof(RankOption.DayMale), RankingsPageResources.RankOptionDayMale)]
[AttachedLocalizedResource(nameof(RankOption.DayFemale), RankingsPageResources.RankOptionDayFemale)]
[AttachedLocalizedResource(nameof(RankOption.DayManga), RankingsPageResources.RankOptionDayManga)]
[AttachedLocalizedResource(nameof(RankOption.WeekManga), RankingsPageResources.RankOptionWeekManga)]
[AttachedLocalizedResource(nameof(RankOption.MonthManga), RankingsPageResources.RankOptionMonthManga)]
[AttachedLocalizedResource(nameof(RankOption.WeekOriginal), RankingsPageResources.RankOptionWeekOriginal)]
[AttachedLocalizedResource(nameof(RankOption.WeekRookie), RankingsPageResources.RankOptionWeekRookie)]
[AttachedLocalizedResource(nameof(RankOption.DayR18), RankingsPageResources.RankOptionDayR18)]
[AttachedLocalizedResource(nameof(RankOption.DayMaleR18), RankingsPageResources.RankOptionDayMaleR18)]
[AttachedLocalizedResource(nameof(RankOption.DayFemaleR18), RankingsPageResources.RankOptionDayFemaleR18)]
[AttachedLocalizedResource(nameof(RankOption.WeekR18), RankingsPageResources.RankOptionWeekR18)]
[AttachedLocalizedResource(nameof(RankOption.WeekR18G), RankingsPageResources.RankOptionWeekR18G)]
[AttachedLocalizedResource(nameof(RankOption.DayAi), RankingsPageResources.RankOptionDayAi)]
[AttachedLocalizedResource(nameof(RankOption.DayR18Ai), RankingsPageResources.RankOptionDayR18Ai)]

[AttachedLocalizationMetadata<RankOption>("Novel")]
[AttachedLocalizedResource(nameof(RankOption.Day), RankingsPageResources.RankOptionDay)]
[AttachedLocalizedResource(nameof(RankOption.Week), RankingsPageResources.RankOptionWeek)]
[AttachedLocalizedResource(nameof(RankOption.DayMale), RankingsPageResources.RankOptionDayMale)]
[AttachedLocalizedResource(nameof(RankOption.DayFemale), RankingsPageResources.RankOptionDayFemale)]
[AttachedLocalizedResource(nameof(RankOption.WeekRookie), RankingsPageResources.RankOptionWeekRookie)]
[AttachedLocalizedResource(nameof(RankOption.DayR18), RankingsPageResources.RankOptionDayR18)]
[AttachedLocalizedResource(nameof(RankOption.DayMaleR18), RankingsPageResources.RankOptionDayMaleR18)]
[AttachedLocalizedResource(nameof(RankOption.DayFemaleR18), RankingsPageResources.RankOptionDayFemaleR18)]
[AttachedLocalizedResource(nameof(RankOption.WeekR18), RankingsPageResources.RankOptionWeekR18)]
[AttachedLocalizedResource(nameof(RankOption.WeekR18G), RankingsPageResources.RankOptionWeekR18G)]
[AttachedLocalizedResource(nameof(RankOption.WeekAi), RankingsPageResources.RankOptionWeekAi)]
[AttachedLocalizedResource(nameof(RankOption.WeekAiR18), RankingsPageResources.RankOptionWeekAiR18)]
public static partial class RankOptionExtension
{
    public static readonly RankOption[] IllustrationRankOptions =
    [
        RankOption.Day,
        RankOption.Week,
        RankOption.Month,
        RankOption.DayMale,
        RankOption.DayFemale,
        RankOption.DayManga,
        RankOption.WeekManga,
        RankOption.MonthManga,
        RankOption.WeekOriginal,
        RankOption.WeekRookie,
        RankOption.DayR18,
        RankOption.DayMaleR18,
        RankOption.DayFemaleR18,
        RankOption.WeekR18,
        RankOption.WeekR18G,
        RankOption.DayAi,
        RankOption.DayR18Ai
        // RankOption.WeekAi,
        // RankOption.WeekAiR18
    ];

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
        // RankOption.WeekOriginal,
        RankOption.WeekRookie,
        RankOption.DayR18,
        RankOption.DayMaleR18,
        RankOption.DayFemaleR18,
        RankOption.WeekR18,
        RankOption.WeekR18G,
        // RankOption.DayAi,
        // RankOption.DayR18Ai,
        RankOption.WeekAi,
        RankOption.WeekAiR18
    ];
}

[AttachedLocalizationMetadata<TargetFilter>]
[AttachedLocalizedResource(nameof(TargetFilter.ForAndroid), MiscResources.TargetFilterForAndroid)]
[AttachedLocalizedResource(nameof(TargetFilter.ForIos), MiscResources.TargetFilterForIOS)]
public static partial class TargetFilterExtension;

[AttachedLocalizationMetadata<SearchIllustrationTagMatchOption>]
[AttachedLocalizedResource(nameof(SearchIllustrationTagMatchOption.PartialMatchForTags), MiscResources.SearchIllustrationTagMatchOptionPartialMatchForTags)]
[AttachedLocalizedResource(nameof(SearchIllustrationTagMatchOption.ExactMatchForTags), MiscResources.SearchIllustrationTagMatchOptionExactMatchForTags)]
[AttachedLocalizedResource(nameof(SearchIllustrationTagMatchOption.TitleAndCaption), MiscResources.SearchIllustrationTagMatchOptionTitleAndCaption)]
public static partial class SearchIllustrationTagMatchOptionExtension;

[AttachedLocalizationMetadata<SearchNovelTagMatchOption>]
[AttachedLocalizedResource(nameof(SearchNovelTagMatchOption.PartialMatchForTags), MiscResources.SearchNovelTagMatchOptionPartialMatchForTags)]
[AttachedLocalizedResource(nameof(SearchNovelTagMatchOption.ExactMatchForTags), MiscResources.SearchNovelTagMatchOptionExactMatchForTags)]
[AttachedLocalizedResource(nameof(SearchNovelTagMatchOption.Text), MiscResources.SearchNovelTagMatchOptionText)]
[AttachedLocalizedResource(nameof(SearchNovelTagMatchOption.Keyword), MiscResources.SearchNovelTagMatchOptionCaption)]
public static partial class SearchNovelTagMatchOptionExtension;

[AttachedLocalizationMetadata<WorkSortOption>]
[AttachedLocalizedResource(Symbol.ArrowSort, nameof(WorkSortOption.DoNotSort), MiscResources.WorkSortOptionDoNotSort)]
[AttachedLocalizedResource(Symbol.ArrowTrendingSparkle, nameof(WorkSortOption.PopularityDescending), MiscResources.WorkSortOptionPopularityDescending)]
[AttachedLocalizedResource(Symbol.ArrowSortUpLines, nameof(WorkSortOption.PublishDateAscending), MiscResources.WorkSortOptionPublishDateAscending)]
[AttachedLocalizedResource(Symbol.ArrowSortDownLines, nameof(WorkSortOption.PublishDateDescending), MiscResources.WorkSortOptionPublishDateDescending)]
public static partial class WorkSortOptionExtension;

[AttachedLocalizationMetadata<PrivacyPolicy>]
[AttachedLocalizedResource(Symbol.Person, nameof(PrivacyPolicy.Public), MiscResources.PrivacyPolicyPublic)]
[AttachedLocalizedResource(Symbol.InPrivateAccount, nameof(PrivacyPolicy.Private), MiscResources.PrivacyPolicyPrivate)]
public static partial class PrivacyPolicyExtension;

[AttachedLocalizationMetadata<WorkType>]
[AttachedLocalizedResource(Symbol.Image, nameof(WorkType.Illustration), MiscResources.WorkTypeIllust)]
[AttachedLocalizedResource(Symbol.ImageStack, nameof(WorkType.Manga), MiscResources.WorkTypeManga)]
[AttachedLocalizedResource(Symbol.Book, nameof(WorkType.Novel), MiscResources.WorkTypeNovel)]
public static partial class WorkTypeExtension;

[AttachedLocalizationMetadata<SimpleWorkType>]  
[AttachedLocalizedResource(Symbol.Image, nameof(SimpleWorkType.IllustrationAndManga), MiscResources.SimpleWorkTypeIllustAndManga)]
[AttachedLocalizedResource(Symbol.Book, nameof(SimpleWorkType.Novel), MiscResources.SimpleWorkTypeNovel)]
public static partial class SimpleWorkTypeExtension;

[AttachedLocalizationMetadata<FontWeight>]
[AttachedLocalizedResource(nameof(FontWeight.Thin), MiscResources.Thin)]
[AttachedLocalizedResource(nameof(FontWeight.ExtraLight), MiscResources.ExtraLight)]
[AttachedLocalizedResource(nameof(FontWeight.Light), MiscResources.Light)]
[AttachedLocalizedResource(nameof(FontWeight.SemiLight), MiscResources.SemiLight)]
[AttachedLocalizedResource(nameof(FontWeight.Normal), MiscResources.Normal)]
[AttachedLocalizedResource(nameof(FontWeight.Medium), MiscResources.Medium)]
[AttachedLocalizedResource(nameof(FontWeight.SemiBold), MiscResources.SemiBold)]
[AttachedLocalizedResource(nameof(FontWeight.Bold), MiscResources.Bold)]
[AttachedLocalizedResource(nameof(FontWeight.ExtraBold), MiscResources.ExtraBold)]
[AttachedLocalizedResource(nameof(FontWeight.Black), MiscResources.Black)]
[AttachedLocalizedResource(nameof(FontWeight.ExtraBlack), MiscResources.ExtraBlack)]
public static partial class FontWeightExtension;


using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.CoreApi.Enums;
using Pixeval.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixeval.Models
{
    internal record UserSetting
    {
        public string UserId { get; init; }
        public ApplicationTheme Theme { get; init; }
        public bool FiltrateRestrictedContent { get; init; }
        public bool DisableDomainFronting { get; init; }
        public IllustrationSortOption DefaultSortOption { get; init; } = IllustrationSortOption.DoNotSort;
        public SearchTagMatchOption TagMatchOption { get; init; } = SearchTagMatchOption.PartialMatchForTags;
        public int PreLoadRows { get; init; } = 2;
        public int PageLimitForKeywordSearch { get; init; } = 100;
        public int SearchStartingFromPageNumber { get; init; } = 1;
        public int PageLimitForSpotlight { get; init; } = 50;
        public string MirrorHost { get; init; } = string.Empty;
        public bool DisplayTeachingTipWhenGeneratingAppLink { get; init; } = true;
        public int ItemsNumberLimitForDailyRecommendations { get; init; } = 500;
        public bool UseFileCache { get; init; } = false;
        public int WindowWidth { get; init; }
        public int WindowHeight { get; init; }
        public ThumbnailDirection ThumbnailDirection { get; init; } = ThumbnailDirection.Portrait;
        public DateTimeOffset LastCheckedUpdate { get; init; } = DateTimeOffset.MinValue;
        public bool DownloadUpdateAutomatically { get; init; } = false;
        public string AppFontFamilyName { get; init; } = "Segoe UI";
        public MainPageTabItem DefaultSelectedTabItem { get; init; } = MainPageTabItem.DailyRecommendation;
        public SearchDuration SearchDuration { get; init; } = SearchDuration.Undecided;
        public bool UsePreciseRangeForSearch { get; init; } = false;
        public DateTimeOffset SearchStartDate { get; init; } = DateTimeOffset.Now - TimeSpan.FromDays(1);
        public DateTimeOffset SearchEndDate { get; init; } = DateTimeOffset.Now;
        public bool OverwriteDownloadedFile { get; init; } = false;
        public string ReverseSearchApiKey { get; init; } = string.Empty;
        public int ReverseSearchResultSimilarityThreshold { get; init; } = 80;
        public int MaximumSuggestionBoxSearchHistory { get; init; } = 10;
    }
}

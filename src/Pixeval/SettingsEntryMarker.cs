#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/SettingsMarker.cs
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
using Pixeval.Attributes;

namespace Pixeval;

// This file contains utilities to be used in the global search system to allow user to search and navigate to setting entries directly in the search bar
// see Pixeval.Pages.SuggestionStateMachine.MatchSettings()

public class SettingsCategory : Attribute
{
    public SettingsEntryCategory Category { get; }

    public SettingsCategory(SettingsEntryCategory category)
    {
        Category = category;
    }

    /**
     * The string name for this category, localized.
     */
    public string Name()
    {
        return Category switch
        {
            SettingsEntryCategory.Version => SettingsPageResources.VersionSettingsGroupHeader,
            SettingsEntryCategory.Session => SettingsPageResources.SessionSettingsGroupHeader,
            SettingsEntryCategory.Application => SettingsPageResources.ApplicationSettingsGroupHeader,
            SettingsEntryCategory.BrowsingExperience => SettingsPageResources.BrowsingExperienceSettingsGroupHeader,
            SettingsEntryCategory.Search => SettingsPageResources.SearchSettingsGroupHeader,
            SettingsEntryCategory.Download => SettingsPageResources.DownloadSettingsGroupHeader,
            SettingsEntryCategory.Misc => SettingsPageResources.MiscSettingsGroupHeader,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public enum SettingsEntryCategory
{
    Version, Session, Application, BrowsingExperience, Search, Download, Misc
}

public enum SettingsEntry
{
    [SettingsCategory(SettingsEntryCategory.Version)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.DownloadUpdateAutomaticallyEntryHeader))]
    AutoUpdate,

    [SettingsCategory(SettingsEntryCategory.Session)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.SignOutEntryHeader))]
    SignOut,

    [SettingsCategory(SettingsEntryCategory.Session)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.ResetDefaultSettingsEntryHeader))]
    ResetSettings,

    [SettingsCategory(SettingsEntryCategory.Application)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.ThemeEntryHeader))]
    AppTheme,

    [SettingsCategory(SettingsEntryCategory.Application)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.AppFontFamilyEntryHeader))]
    Font,

    [SettingsCategory(SettingsEntryCategory.Application)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.DisableDomainFrontingEntryHeader))]
    DomainFronting,

    [SettingsCategory(SettingsEntryCategory.Application)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.UseFileCacheEntryHeader))]
    FileCache,

    [SettingsCategory(SettingsEntryCategory.Application)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.GenerateHelpLinkEntryHeader))]
    GenerateHelpLink,

    [SettingsCategory(SettingsEntryCategory.Application)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSelectedTabEntryHeader))]
    DefaultTab,

    [SettingsCategory(SettingsEntryCategory.BrowsingExperience)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.ThumbnailDirectionEntryHeader))]
    ThumbnailDirection,

    [SettingsCategory(SettingsEntryCategory.BrowsingExperience)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.FiltrateRestrictedContentEntryHeader))]
    FilterSensContent,

    [SettingsCategory(SettingsEntryCategory.BrowsingExperience)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.TargetAPIPlatformEntryHeader))]
    ApiPlatform,

    [SettingsCategory(SettingsEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.ReverseSearchResultSimilarityThresholdEntryHeader))]
    ReverseSearchThreshold,

    [SettingsCategory(SettingsEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.ReverseSearchApiKeyEntryHeader))]
    ReverseSearchApiKey,

    [SettingsCategory(SettingsEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumSearchHistoryRecordsEntryHeader))]
    SearchHistoryLimit,

    [SettingsCategory(SettingsEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumSuggestionBoxSearchHistoryEntryHeader))]
    SearchBoxHistoryLimit,

    [SettingsCategory(SettingsEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSearchSortOptionEntryHeader))]
    DefaultSortOption,

    [SettingsCategory(SettingsEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSearchTagMatchOptionEntryHeader))]
    DefaultTagMatchPolicy,

    [SettingsCategory(SettingsEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumSearchPageLimitHeader))]
    MaxSearchPages,

    [SettingsCategory(SettingsEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.SearchStartsFromEntryHeader))]
    SearchStartsFrom,

    [SettingsCategory(SettingsEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.SearchDurationEntryHeader))]
    SearchRange,

    [SettingsCategory(SettingsEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.UsePreciseRangeForSearchEntryHeader))]
    UsePreciseSearchRange,

    [SettingsCategory(SettingsEntryCategory.Download)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumDownloadHistoryRecordsEntryHeader))]
    DownloadHistoryLimit,

    [SettingsCategory(SettingsEntryCategory.Download)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.OverwriteDownloadedFileEntryHeader))]
    OverwriteDownloadedFile,

    [SettingsCategory(SettingsEntryCategory.Download)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.MaxDownloadConcurrencyLevelEntryHeader))]
    MaximumParallelism,

    [SettingsCategory(SettingsEntryCategory.Download)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultDownloadPathMacroEntryHeader))]
    DefaultDownloadPath,

    [SettingsCategory(SettingsEntryCategory.Misc)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.SpotlightSearchPageLimitEntryHeader))]
    SpotlightSearchPageLimit,

    [SettingsCategory(SettingsEntryCategory.Misc)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.RecommendationItemLimitEntryHeader))]
    RecommendationResultLimit,

    [SettingsCategory(SettingsEntryCategory.Misc)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumBrowseHistoryRecordsEntryHeader))]
    BrowseHistoryLimit,

    [SettingsCategory(SettingsEntryCategory.Misc)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.ImageMirrorServerEntryHeader))]
    ImageMirrorServer,

    [SettingsCategory(SettingsEntryCategory.Misc)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.PreloadRowsEntryHeader))]
    PreloadRows,

    [SettingsCategory(SettingsEntryCategory.BrowsingExperience)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.IllustrationViewOptionEntryHeader))]
    IllustrationViewOption
}

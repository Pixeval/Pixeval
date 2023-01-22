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
    public SettingEntryCategory Category { get; }

    public SettingsCategory(SettingEntryCategory category)
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
            SettingEntryCategory.Version => SettingsPageResources.VersionSettingsGroupHeader,
            SettingEntryCategory.Session => SettingsPageResources.SessionSettingsGroupHeader,
            SettingEntryCategory.Application => SettingsPageResources.ApplicationSettingsGroupHeader,
            SettingEntryCategory.BrowsingExperience => SettingsPageResources.BrowsingExperienceSettingsGroupHeader,
            SettingEntryCategory.Search => SettingsPageResources.SearchSettingsGroupHeader,
            SettingEntryCategory.Download => SettingsPageResources.DownloadSettingsGroupHeader,
            SettingEntryCategory.Misc => SettingsPageResources.MiscSettingsGroupHeader,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public enum SettingEntryCategory
{
    Version, Session, Application, BrowsingExperience, Search, Download, Misc
}

public partial record SettingEntry(SettingEntryCategory Category, Type ResourceType, string ResourceKey)
{
    public static readonly SettingEntry AutoUpdate = new(SettingEntryCategory.Version, typeof(SettingsPageResources), nameof(SettingsPageResources.DownloadUpdateAutomaticallyEntryHeader));

    public static readonly SettingEntry SignOut = new(SettingEntryCategory.Session, typeof(SettingsPageResources), nameof(SettingsPageResources.SignOutEntryHeader));

    public static readonly SettingEntry ResetSettings = new(SettingEntryCategory.Session, typeof(SettingsPageResources), nameof(SettingsPageResources.ResetDefaultSettingsEntryHeader));
}

public enum SettingsEntry
{
    [SettingsCategory(SettingEntryCategory.Version)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.DownloadUpdateAutomaticallyEntryHeader))]
    AutoUpdate,

    [SettingsCategory(SettingEntryCategory.Session)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.SignOutEntryHeader))]
    SignOut,

    [SettingsCategory(SettingEntryCategory.Session)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.ResetDefaultSettingsEntryHeader))]
    ResetSettings,

    [SettingsCategory(SettingEntryCategory.Application)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.ThemeEntryHeader))]
    AppTheme,

    [SettingsCategory(SettingEntryCategory.Application)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.AppFontFamilyEntryHeader))]
    Font,

    [SettingsCategory(SettingEntryCategory.Application)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.DisableDomainFrontingEntryHeader))]
    DomainFronting,

    [SettingsCategory(SettingEntryCategory.Application)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.UseFileCacheEntryHeader))]
    FileCache,

    [SettingsCategory(SettingEntryCategory.Application)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.GenerateHelpLinkEntryHeader))]
    GenerateHelpLink,

    [SettingsCategory(SettingEntryCategory.Application)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSelectedTabEntryHeader))]
    DefaultTab,

    [SettingsCategory(SettingEntryCategory.BrowsingExperience)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.ThumbnailDirectionEntryHeader))]
    ThumbnailDirection,

    [SettingsCategory(SettingEntryCategory.BrowsingExperience)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.FiltrateRestrictedContentEntryHeader))]
    FilterSensContent,

    [SettingsCategory(SettingEntryCategory.BrowsingExperience)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.TargetAPIPlatformEntryHeader))]
    ApiPlatform,

    [SettingsCategory(SettingEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.ReverseSearchResultSimilarityThresholdEntryHeader))]
    ReverseSearchThreshold,

    [SettingsCategory(SettingEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.ReverseSearchApiKeyEntryHeader))]
    ReverseSearchApiKey,

    [SettingsCategory(SettingEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumSearchHistoryRecordsEntryHeader))]
    SearchHistoryLimit,

    [SettingsCategory(SettingEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumSuggestionBoxSearchHistoryEntryHeader))]
    SearchBoxHistoryLimit,

    [SettingsCategory(SettingEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSearchSortOptionEntryHeader))]
    DefaultSortOption,

    [SettingsCategory(SettingEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultSearchTagMatchOptionEntryHeader))]
    DefaultTagMatchPolicy,

    [SettingsCategory(SettingEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumSearchPageLimitHeader))]
    MaxSearchPages,

    [SettingsCategory(SettingEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.SearchStartsFromEntryHeader))]
    SearchStartsFrom,

    [SettingsCategory(SettingEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.SearchDurationEntryHeader))]
    SearchRange,

    [SettingsCategory(SettingEntryCategory.Search)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.UsePreciseRangeForSearchEntryHeader))]
    UsePreciseSearchRange,

    [SettingsCategory(SettingEntryCategory.Download)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumDownloadHistoryRecordsEntryHeader))]
    DownloadHistoryLimit,

    [SettingsCategory(SettingEntryCategory.Download)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.OverwriteDownloadedFileEntryHeader))]
    OverwriteDownloadedFile,

    [SettingsCategory(SettingEntryCategory.Download)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.MaxDownloadConcurrencyLevelEntryHeader))]
    MaximumParallelism,

    [SettingsCategory(SettingEntryCategory.Download)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.DefaultDownloadPathMacroEntryHeader))]
    DefaultDownloadPath,

    [SettingsCategory(SettingEntryCategory.Misc)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.SpotlightSearchPageLimitEntryHeader))]
    SpotlightSearchPageLimit,

    [SettingsCategory(SettingEntryCategory.Misc)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.RecommendationItemLimitEntryHeader))]
    RecommendationResultLimit,

    [SettingsCategory(SettingEntryCategory.Misc)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.MaximumBrowseHistoryRecordsEntryHeader))]
    BrowseHistoryLimit,

    [SettingsCategory(SettingEntryCategory.Misc)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.ImageMirrorServerEntryHeader))]
    ImageMirrorServer,

    [SettingsCategory(SettingEntryCategory.Misc)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.PreloadRowsEntryHeader))]
    PreloadRows,

    [SettingsCategory(SettingEntryCategory.BrowsingExperience)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.IllustrationViewOptionEntryHeader))]
    IllustrationViewOption,

    [SettingsCategory(SettingEntryCategory.Application)]
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.BackdropEntryHeader))]
    AppBackdrop,
}

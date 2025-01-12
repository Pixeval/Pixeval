// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Pages.Misc;

[LocalizationMetadata(typeof(SettingsPageResources))]
public enum ClearDataKind
{
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.FileCacheCleared))]
    FileCache,

    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.BrowseHistoriesCleared))]
    BrowseHistory,

    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.SearchHistoriesCleared))]
    SearchHistory,

    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.DownloadHistoriesCleared))]
    DownloadHistory
}

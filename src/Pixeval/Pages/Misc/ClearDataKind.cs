// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Pages.Misc;

[LocalizationMetadata(typeof(SettingsPageResources))]
public enum ClearDataKind
{
    [LocalizedResource(nameof(SettingsPageResources.FileCacheCleared))]
    FileCache,

    [LocalizedResource(nameof(SettingsPageResources.BrowseHistoriesCleared))]
    BrowseHistory,

    [LocalizedResource(nameof(SettingsPageResources.SearchHistoriesCleared))]
    SearchHistory,

    [LocalizedResource(nameof(SettingsPageResources.DownloadHistoriesCleared))]
    DownloadHistory
}

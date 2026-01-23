// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
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

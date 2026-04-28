// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum ClearDataKind
{
    [LocalizedResource(nameof(SettingsMainViewResources.FileCacheCleared))]
    FileCache,

    [LocalizedResource(nameof(SettingsMainViewResources.BrowseHistoriesCleared))]
    BrowseHistory,

    [LocalizedResource(nameof(SettingsMainViewResources.SearchHistoriesCleared))]
    SearchHistory,

    [LocalizedResource(nameof(SettingsMainViewResources.DownloadHistoriesCleared))]
    DownloadHistory
}

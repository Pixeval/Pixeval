// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Controls;

[LocalizationMetadata(typeof(DownloadPageResources))]
public enum DownloadListOption
{
    [LocalizedResource(nameof(DownloadPageResources.DownloadListOptionAllQueued))]
    AllQueued,

    [LocalizedResource(nameof(DownloadPageResources.DownloadListOptionRunning))]
    Running,

    [LocalizedResource(nameof(DownloadPageResources.DownloadListOptionCompleted))]
    Completed,

    [LocalizedResource(nameof(DownloadPageResources.DownloadListOptionCancelled))]
    Cancelled,

    [LocalizedResource(nameof(DownloadPageResources.DownloadListOptionError))]
    Error,

    [LocalizedResource(nameof(DownloadPageResources.DownloadListOptionCustomSearch))]
    CustomSearch
}

// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Controls;

[LocalizationMetadata(typeof(DownloadPageResources))]
public enum DownloadListOption
{
    [LocalizedResource(typeof(DownloadPageResources), nameof(DownloadPageResources.DownloadListOptionAllQueued))]
    AllQueued,

    [LocalizedResource(typeof(DownloadPageResources), nameof(DownloadPageResources.DownloadListOptionRunning))]
    Running,

    [LocalizedResource(typeof(DownloadPageResources), nameof(DownloadPageResources.DownloadListOptionCompleted))]
    Completed,

    [LocalizedResource(typeof(DownloadPageResources), nameof(DownloadPageResources.DownloadListOptionCancelled))]
    Cancelled,

    [LocalizedResource(typeof(DownloadPageResources), nameof(DownloadPageResources.DownloadListOptionError))]
    Error,

    [LocalizedResource(typeof(DownloadPageResources), nameof(DownloadPageResources.DownloadListOptionCustomSearch))]
    CustomSearch
}

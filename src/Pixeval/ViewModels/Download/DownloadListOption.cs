// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.ViewModels;

[LocalizationMetadata]
public enum DownloadListOption
{
    [LocalizedResource(DownloadPageResources.DownloadListOptionAllQueued)]
    AllQueued,

    [LocalizedResource(DownloadPageResources.DownloadListOptionRunning)]
    Running,

    [LocalizedResource(DownloadPageResources.DownloadListOptionCompleted)]
    Completed,

    [LocalizedResource(DownloadPageResources.DownloadListOptionCancelled)]
    Cancelled,

    [LocalizedResource(DownloadPageResources.DownloadListOptionError)]
    Error,

    [LocalizedResource(DownloadPageResources.DownloadListOptionCustomSearch)]
    CustomSearch
}

// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum DownloadListOption
{
    [LocalizedResource(EnumResources.DownloadListOptionAllQueued)]
    AllQueued,

    [LocalizedResource(EnumResources.DownloadListOptionRunning)]
    Running,

    [LocalizedResource(EnumResources.DownloadListOptionCompleted)]
    Completed,

    [LocalizedResource(EnumResources.DownloadListOptionCancelled)]
    Cancelled,

    [LocalizedResource(EnumResources.DownloadListOptionError)]
    Error,

    [LocalizedResource(EnumResources.DownloadListOptionCustomSearch)]
    CustomSearch
}

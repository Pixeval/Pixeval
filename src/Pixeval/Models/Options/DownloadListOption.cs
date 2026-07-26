// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum DownloadListOption
{
    [LocalizedResource(EnumResources.DownloadListOption.AllQueued)]
    AllQueued,

    [LocalizedResource(EnumResources.DownloadListOption.Running)]
    Running,

    [LocalizedResource(EnumResources.DownloadListOption.Completed)]
    Completed,

    [LocalizedResource(EnumResources.DownloadListOption.Cancelled)]
    Cancelled,

    [LocalizedResource(EnumResources.DownloadListOption.Error)]
    Error,

    [LocalizedResource(EnumResources.DownloadListOption.CustomSearch)]
    CustomSearch
}

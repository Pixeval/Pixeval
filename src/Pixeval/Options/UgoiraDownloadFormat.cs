// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Options;

[LocalizationMetadata(typeof(MiscResources))]
public enum UgoiraDownloadFormat
{
    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Tiff))]
    Tiff,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Png))]
    APng,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Gif))]
    Gif,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.WebPLossy))]
    WebPLossy,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.WebPLossless))]
    WebPLossless,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.OriginalUgoira))]
    Original
}

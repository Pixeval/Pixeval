// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Options;

[LocalizationMetadata(typeof(MiscResources))]
public enum UgoiraDownloadFormat
{
    [LocalizedResource(nameof(MiscResources.Tiff))]
    Tiff,

    [LocalizedResource(nameof(MiscResources.Png))]
    APng,

    [LocalizedResource(nameof(MiscResources.Gif))]
    Gif,

    [LocalizedResource(nameof(MiscResources.WebPLossy))]
    WebPLossy,

    [LocalizedResource(nameof(MiscResources.WebPLossless))]
    WebPLossless,

    [LocalizedResource(nameof(MiscResources.OriginalUgoira))]
    Original
}

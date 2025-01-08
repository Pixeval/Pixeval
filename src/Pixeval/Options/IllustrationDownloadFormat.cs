// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Options;

[LocalizationMetadata(typeof(MiscResources))]
public enum IllustrationDownloadFormat
{
    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Jpg))]
    Jpg,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Png))]
    Png,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Bmp))]
    Bmp,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.WebPLossy))]
    WebPLossy,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.WebPLossless))]
    WebPLossless,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Original))]
    Original,
}

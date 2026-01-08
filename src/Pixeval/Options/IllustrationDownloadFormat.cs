// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Options;

[LocalizationMetadata(typeof(MiscResources))]
public enum IllustrationDownloadFormat
{
    [LocalizedResource(nameof(MiscResources.Jpg))]
    Jpg,

    [LocalizedResource(nameof(MiscResources.Png))]
    Png,

    [LocalizedResource(nameof(MiscResources.Bmp))]
    Bmp,

    [LocalizedResource(nameof(MiscResources.WebPLossy))]
    WebPLossy,

    [LocalizedResource(nameof(MiscResources.WebPLossless))]
    WebPLossless,

    [LocalizedResource(nameof(MiscResources.OriginalIllustration))]
    Original,
}

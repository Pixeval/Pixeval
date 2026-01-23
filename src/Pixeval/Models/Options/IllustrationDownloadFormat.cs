// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum IllustrationDownloadFormat
{
    [LocalizedResource(MiscResources.Jpg)]
    Jpg,

    [LocalizedResource(MiscResources.Png)]
    Png,

    [LocalizedResource(MiscResources.Bmp)]
    Bmp,

    [LocalizedResource(MiscResources.WebPLossy)]
    WebPLossy,

    [LocalizedResource(MiscResources.WebPLossless)]
    WebPLossless,

    [LocalizedResource(MiscResources.OriginalIllustration)]
    Original
}

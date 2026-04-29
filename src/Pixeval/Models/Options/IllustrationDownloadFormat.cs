// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum IllustrationDownloadFormat
{
    [LocalizedResource(Resource = "JPG")]
    Jpg,

    [LocalizedResource(Resource = "PNG")]
    Png,

    [LocalizedResource(Resource = "BMP")]
    Bmp,

    [LocalizedResource(EnumResources.IllustrationDownloadFormatWebPLossy)]
    WebPLossy,

    [LocalizedResource(EnumResources.IllustrationDownloadFormatWebPLossless)]
    WebPLossless,

    [LocalizedResource(EnumResources.IllustrationDownloadFormatOriginal)]
    Original
}

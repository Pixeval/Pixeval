// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum UgoiraDownloadFormat
{
    [LocalizedResource(Resource = "TIFF")]
    Tiff,

    [LocalizedResource(Resource = "APNG")]
    APng,

    [LocalizedResource(Resource = "GIF")]
    Gif,

    [LocalizedResource(EnumResources.UgoiraDownloadFormatWebPLossy)]
    WebPLossy,

    [LocalizedResource(EnumResources.UgoiraDownloadFormatWebPLossless)]
    WebPLossless,

    [LocalizedResource(EnumResources.UgoiraDownloadFormatOriginal)]
    Original
}

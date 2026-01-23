// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum UgoiraDownloadFormat
{
    [LocalizedResource(MiscResources.Tiff)]
    Tiff,

    [LocalizedResource(MiscResources.Png)]
    APng,

    [LocalizedResource(MiscResources.Gif)]
    Gif,

    [LocalizedResource(MiscResources.WebPLossy)]
    WebPLossy,

    [LocalizedResource(MiscResources.WebPLossless)]
    WebPLossless,

    [LocalizedResource(MiscResources.OriginalUgoira)]
    Original
}

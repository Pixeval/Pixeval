using Pixeval.Attributes;

namespace Pixeval.Options;

public enum UgoiraDownloadFormat
{
    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Tiff))]
    Tiff,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.APng))]
    APng,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Gif))]
    Gif,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.WebP))]
    WebP
}

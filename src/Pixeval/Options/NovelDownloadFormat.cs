// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Options;

[LocalizationMetadata(typeof(MiscResources))]
public enum NovelDownloadFormat
{
    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Pdf))]
    Pdf,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Html))]
    Html,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Md))]
    Md,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.OriginalTxt))]
    OriginalTxt
}

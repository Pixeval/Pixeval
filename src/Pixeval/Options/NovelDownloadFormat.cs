// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Options;

[LocalizationMetadata(typeof(MiscResources))]
public enum NovelDownloadFormat
{
    [LocalizedResource(nameof(MiscResources.Pdf))]
    Pdf,

    [LocalizedResource(nameof(MiscResources.Html))]
    Html,

    [LocalizedResource(nameof(MiscResources.Md))]
    Md,

    [LocalizedResource(nameof(MiscResources.OriginalTxt))]
    OriginalTxt
}

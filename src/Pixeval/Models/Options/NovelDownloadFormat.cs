// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum NovelDownloadFormat
{
    [LocalizedResource(MiscResources.Pdf)]
    Pdf,

    [LocalizedResource(MiscResources.Html)]
    Html,

    [LocalizedResource(MiscResources.Md)]
    Md,

    [LocalizedResource(MiscResources.OriginalTxt)]
    OriginalTxt
}

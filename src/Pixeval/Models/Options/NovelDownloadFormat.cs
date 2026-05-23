// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum NovelDownloadFormat
{
    [LocalizedResource(EnumResources.NovelDownloadFormatHtml)]
    Html,

    [LocalizedResource(EnumResources.NovelDownloadFormatMd)]
    Md,

    [LocalizedResource(EnumResources.NovelDownloadFormatOriginalTxt)]
    OriginalTxt
}

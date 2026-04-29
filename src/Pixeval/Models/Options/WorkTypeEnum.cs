// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum WorkTypeEnum
{
    [LocalizedResource(EnumResources.WorkTypeEnumIllustration)]
    Illustration,

    [LocalizedResource(EnumResources.WorkTypeEnumManga)]
    Manga,

    [LocalizedResource(EnumResources.WorkTypeEnumUgoira)]
    Ugoira,

    [LocalizedResource(EnumResources.WorkTypeEnumNovel)]
    Novel
}

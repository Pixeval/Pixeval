// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum WorkTypeEnum
{
    [LocalizedResource(EnumResources.WorkTypeEnum.Illustration)]
    Illustration,

    [LocalizedResource(EnumResources.WorkTypeEnum.Manga)]
    Manga,

    [LocalizedResource(EnumResources.WorkTypeEnum.Ugoira)]
    Ugoira,

    [LocalizedResource(EnumResources.WorkTypeEnum.Novel)]
    Novel
}

// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum WorkTypeEnum
{
    [LocalizedResource(SettingsPageResources.IllustrationOptionEntryHeader)]
    Illustration,

    [LocalizedResource(SettingsPageResources.MangaOptionEntryHeader)]
    Manga,

    [LocalizedResource(SettingsPageResources.UgoiraOptionEntryHeader)]
    Ugoira,

    [LocalizedResource(SettingsPageResources.NovelOptionEntryHeader)]
    Novel
}

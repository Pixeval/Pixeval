// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Options;

[LocalizationMetadata(typeof(SettingsPageResources))]
public enum WorkTypeEnum
{
    [LocalizedResource(nameof(SettingsPageResources.IllustrationOptionEntryHeader))]
    Illustration,

    [LocalizedResource(nameof(SettingsPageResources.MangaOptionEntryHeader))]
    Manga,

    [LocalizedResource(nameof(SettingsPageResources.UgoiraOptionEntryHeader))]
    Ugoira,

    [LocalizedResource(nameof(SettingsPageResources.NovelOptionEntryHeader))]
    Novel
}

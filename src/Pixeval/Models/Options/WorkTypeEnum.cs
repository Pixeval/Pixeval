// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum WorkTypeEnum
{
    [LocalizedResource(SettingsMainViewResources.IllustrationOptionEntryHeader)]
    Illustration,

    [LocalizedResource(SettingsMainViewResources.MangaOptionEntryHeader)]
    Manga,

    [LocalizedResource(SettingsMainViewResources.UgoiraOptionEntryHeader)]
    Ugoira,

    [LocalizedResource(SettingsMainViewResources.NovelOptionEntryHeader)]
    Novel
}

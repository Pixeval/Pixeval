// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using FluentIcons.Common;
using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum SettingsEntryCategory
{
    [LocalizedResource(Symbol.DesignIdeas, SettingsPageResources.VersionSettingsGroupText)]
    Version,

    [LocalizedResource(Symbol.Person, SettingsPageResources.SessionSettingsGroupText)]
    Session,

    [LocalizedResource(Symbol.Apps, SettingsPageResources.ApplicationSettingsGroupText)]
    Application,

    [LocalizedResource(Symbol.News, SettingsPageResources.BrowsingExperienceSettingsGroupText)]
    BrowsingExperience,

    [LocalizedResource(Symbol.SearchSparkle, SettingsPageResources.SearchSettingsGroupText)]
    Search,

    [LocalizedResource(Symbol.ArrowSquareDown, SettingsPageResources.DownloadSettingsGroupText)]
    Download,

    [LocalizedResource(Symbol.WrenchScrewdriver, SettingsPageResources.MiscSettingsGroupText)]
    Misc
}

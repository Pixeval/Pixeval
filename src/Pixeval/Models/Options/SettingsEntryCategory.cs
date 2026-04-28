// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using FluentIcons.Common;
using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum SettingsEntryCategory
{
    [LocalizedResource(Symbol.DesignIdeas, SettingsMainViewResources.VersionSettingsGroupText)]
    Version,

    [LocalizedResource(Symbol.Person, SettingsMainViewResources.SessionSettingsGroupText)]
    Session,

    [LocalizedResource(Symbol.Apps, SettingsMainViewResources.ApplicationSettingsGroupText)]
    Application,

    [LocalizedResource(Symbol.News, SettingsMainViewResources.BrowsingExperienceSettingsGroupText)]
    BrowsingExperience,

    [LocalizedResource(Symbol.SearchSparkle, SettingsMainViewResources.SearchSettingsGroupText)]
    Search,

    [LocalizedResource(Symbol.ArrowSquareDown, SettingsMainViewResources.DownloadSettingsGroupText)]
    Download,

    [LocalizedResource(Symbol.WrenchScrewdriver, SettingsMainViewResources.MiscSettingsGroupText)]
    Misc
}

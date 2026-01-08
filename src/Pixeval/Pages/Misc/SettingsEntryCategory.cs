// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Pages.Misc;

[LocalizationMetadata(typeof(SettingsPageResources))]
public enum SettingsEntryCategory
{
    [LocalizedResource(nameof(SettingsPageResources.VersionSettingsGroupText))]
    Version,

    [LocalizedResource(nameof(SettingsPageResources.SessionSettingsGroupText))]
    Session,

    [LocalizedResource(nameof(SettingsPageResources.ApplicationSettingsGroupText))]
    Application,

    [LocalizedResource(nameof(SettingsPageResources.BrowsingExperienceSettingsGroupText))]
    BrowsingExperience,

    [LocalizedResource(nameof(SettingsPageResources.SearchSettingsGroupText))]
    Search,

    [LocalizedResource(nameof(SettingsPageResources.DownloadSettingsGroupText))]
    Download,

    [LocalizedResource(nameof(SettingsPageResources.MiscSettingsGroupText))]
    Misc
}

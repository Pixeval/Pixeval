// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Pages.Misc;

[LocalizationMetadata(typeof(SettingsPageResources))]
public enum SettingsEntryCategory
{
    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.VersionSettingsGroupText))]
    Version,

    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.SessionSettingsGroupText))]
    Session,

    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.ApplicationSettingsGroupText))]
    Application,

    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.BrowsingExperienceSettingsGroupText))]
    BrowsingExperience,

    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.SearchSettingsGroupText))]
    Search,

    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.DownloadSettingsGroupText))]
    Download,

    [LocalizedResource(typeof(SettingsPageResources), nameof(SettingsPageResources.MiscSettingsGroupText))]
    Misc
}

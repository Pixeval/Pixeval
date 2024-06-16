#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/SettingsEntryCategory.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

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

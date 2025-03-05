// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Pixeval.AppManagement;
using Pixeval.Pages.Misc;

namespace Pixeval.Settings;

public partial class SimpleSettingsGroup(SettingsEntryCategory tag) : List<IAppSettingEntry<AppSettings>>, ISettingsGroup
{
    public SettingsEntryCategory Tag { get; } = tag;

    public string Header { get; } = SettingsEntryCategoryExtension.GetResource(tag);

    IEnumerator<ISettingsEntry> IEnumerable<ISettingsEntry>.GetEnumerator() => ((IEnumerable<IAppSettingEntry<AppSettings>>) this).GetEnumerator();
}

using System.Collections.Generic;
using Pixeval.Pages.Misc;

namespace Pixeval.Settings;

public partial class SimpleSettingsGroup(SettingsEntryCategory tag) : List<ISettingsEntry>, ISettingsGroup
{
    public string Header { get; } = SettingsEntryCategoryExtension.GetResource(tag);

    public SettingsEntryCategory Tag { get; } = tag;
}

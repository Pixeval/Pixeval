using System.Collections.Generic;
using Pixeval.AppManagement;
using WinUI3Utilities.Controls;

namespace Pixeval.Settings;

public abstract class MultiValuesSettingsEntryBase<TSettings>(
    TSettings settings,
    string header,
    string description,
    IconGlyph headerIcon,
    IReadOnlyList<SingleValueSettingsEntryBase<AppSettings>> entries) : SettingsEntryBase<TSettings>(settings, header, description, headerIcon)
{
    public IReadOnlyList<SingleValueSettingsEntryBase<AppSettings>> Entries { get; } = entries;

    public override void ValueReset()
    {
        foreach (var entry in Entries)
            entry.ValueReset();
    }
}

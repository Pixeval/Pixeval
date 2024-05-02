using System.Collections.Generic;
using WinUI3Utilities.Controls;

namespace Pixeval.Settings;

public abstract class MultiValuesSettingsEntryBase<TSettings>(
    TSettings settings,
    string header,
    string description,
    IconGlyph headerIcon,
    IReadOnlyList<SingleValueSettingsEntryBase<TSettings>> entries) : SettingsEntryBase<TSettings>(settings, header, description, headerIcon)
{
    public IReadOnlyList<SingleValueSettingsEntryBase<TSettings>> Entries { get; } = entries;

    public override void ValueReset()
    {
        foreach (var entry in Entries)
            entry.ValueReset();
    }
}
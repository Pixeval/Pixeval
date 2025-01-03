using System.Collections.Generic;
using FluentIcons.Common;

namespace Pixeval.Settings;

public abstract class MultiValuesSettingsEntryBase<TSettings>(
    TSettings settings,
    string header,
    string description,
    Symbol headerIcon,
    IReadOnlyList<SingleValueSettingsEntryBase<TSettings>> entries) : SettingsEntryBase(header, description, headerIcon)
{
    public TSettings Settings { get; } = settings;

    public IReadOnlyList<SingleValueSettingsEntryBase<TSettings>> Entries { get; } = entries;

    public override void ValueReset()
    {
        foreach (var entry in Entries)
            entry.ValueReset();
    }
}

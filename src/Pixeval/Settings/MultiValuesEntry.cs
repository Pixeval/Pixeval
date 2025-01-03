using System.Collections.Generic;
using FluentIcons.Common;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings;

public class MultiValuesEntry(
    string header,
    string description,
    Symbol headerIcon,
    IReadOnlyList<ISettingsEntry> entries) : SettingsEntryBase(header, description, headerIcon)
{
    public override MultiValuesAppSettingsExpander Element => new() { Entry = this };

    public IReadOnlyList<ISettingsEntry> Entries { get; } = entries;

    public override void ValueReset()
    {
        foreach (var entry in Entries)
            entry.ValueReset();
    }

    public override void ValueSaving()
    {
        foreach (var entry in Entries)
            entry.ValueSaving();
    }
}

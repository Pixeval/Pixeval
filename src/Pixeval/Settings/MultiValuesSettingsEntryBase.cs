using System.Collections.Generic;
using FluentIcons.Common;
using Pixeval.AppManagement;

namespace Pixeval.Settings;

public abstract class MultiValuesSettingsEntryBase<TSettings>(
    TSettings settings,
    string header,
    string description,
    Symbol headerIcon,
    IReadOnlyList<SingleValueSettingsEntryBase<AppSettings>> entries) : SettingsEntryBase<TSettings>(settings, header, description, headerIcon)
{
    public IReadOnlyList<SingleValueSettingsEntryBase<AppSettings>> Entries { get; } = entries;

    public override void ValueReset()
    {
        foreach (var entry in Entries)
            entry.ValueReset();
    }
}

using System.Collections.Generic;
using Windows.Foundation.Collections;
using FluentIcons.Common;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings;

public class MultiValuesEntry(
    string header,
    string description,
    Symbol headerIcon,
    IReadOnlyList<IAppSettingEntry<AppSettings>> entries) 
    : SettingsEntryBase(header, description, headerIcon), IAppSettingEntry<AppSettings>
{
    public override MultiValuesAppSettingsExpander Element => new() { Entry = this };

    public IReadOnlyList<IAppSettingEntry<AppSettings>> Entries { get; } = entries;

    public void ValueReset(AppSettings defaultSettings)
    {
        foreach (var entry in Entries)
            entry.ValueReset(defaultSettings);
    }

    public override void ValueSaving(IPropertySet values)
    {
        foreach (var entry in Entries)
            entry.ValueSaving(values);
    }
}

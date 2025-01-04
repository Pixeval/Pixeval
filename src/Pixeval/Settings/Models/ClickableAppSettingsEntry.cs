using System;
using Windows.Foundation.Collections;
using FluentIcons.Common;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public class ClickableAppSettingsEntry(
    string header,
    string description,
    Symbol headerIcon,
    Action clicked)
    : SettingsEntryBase(header, description, headerIcon), IAppSettingEntry<AppSettings>
{
    public override ClickableSettingsCard Element => new() { Entry = this };

    public Action Clicked { get; set; } = clicked;

    public Symbol ActionIcon { get; set; } = Symbol.Open;

    public void ValueReset(AppSettings defaultSetting)
    {
    }

    public override void ValueSaving(IPropertySet values)
    {
    }
}

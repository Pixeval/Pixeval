// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using FluentIcons.Common;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;
using Windows.Foundation.Collections;

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

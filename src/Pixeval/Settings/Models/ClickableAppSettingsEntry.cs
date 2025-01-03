using System;
using FluentIcons.Common;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public class ClickableAppSettingsEntry(
    string header,
    string description,
    Symbol headerIcon,
    Action clicked)
    : SettingsEntryBase(header, description, headerIcon)
{
    public override ClickableSettingsCard Element => new() { Entry = this };

    public Action Clicked { get; set; } = clicked;

    public Symbol ActionIcon { get; set; } = Symbol.Open;

    public override void ValueReset() { }
}

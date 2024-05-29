using System;
using FluentIcons.Common;

namespace Pixeval.Settings;

public abstract class ClickableSettingsEntryBase<TSettings>(
    TSettings settings,
    string header,
    string description,
    Symbol headerIcon,
    Action clicked)
    : SettingsEntryBase<TSettings>(settings, header, description, headerIcon)
{
    public Action Clicked { get; set; } = clicked;

    public Symbol ActionIcon { get; set; } = Symbol.Open;

    public override void ValueReset() { }
}

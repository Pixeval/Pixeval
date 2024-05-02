using System;
using WinUI3Utilities.Controls;

namespace Pixeval.Settings;

public abstract class ClickableSettingsEntryBase<TSettings>(
    TSettings settings,
    string header,
    string description,
    IconGlyph headerIcon,
    Action clicked)
    : SettingsEntryBase<TSettings>(settings, header, description, headerIcon)
{
    public Action Clicked { get; set; } = clicked;

    public IconGlyph ActionIcon { get; set; } = IconGlyph.OpenInNewWindowE8A7;

    public override void ValueReset() { }
}
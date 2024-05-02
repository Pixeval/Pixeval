using System;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;
using WinUI3Utilities.Controls;

namespace Pixeval.Settings.Models;

public class ClickableAppSettingsEntry(
    AppSettings settings,
    string header,
    string description,
    IconGlyph headerIcon,
    Action clicked)
    : ClickableSettingsEntryBase<AppSettings>(settings, header, description, headerIcon, clicked)
{
    public override ClickableSettingsCard Element => new() { Entry = this };
}
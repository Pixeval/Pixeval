using System;
using FluentIcons.Common;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public class ClickableAppSettingsEntry(
    AppSettings settings,
    string header,
    string description,
    Symbol headerIcon,
    Action clicked)
    : ClickableSettingsEntryBase<AppSettings>(settings, header, description, headerIcon, clicked)
{
    public override ClickableSettingsCard Element => new() { Entry = this };
}

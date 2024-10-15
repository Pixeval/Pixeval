using System.Collections.Generic;
using FluentIcons.Common;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public class MultiValuesAppSettingsEntry(
    AppSettings appSettings,
    string header,
    string description,
    Symbol headerIcon,
    IReadOnlyList<SingleValueSettingsEntryBase<AppSettings>> entries)
    : MultiValuesSettingsEntryBase<AppSettings>(appSettings, header, description, headerIcon, entries)
{
    public override MultiValuesAppSettingsExpander Element => new() { Entry = this };
}

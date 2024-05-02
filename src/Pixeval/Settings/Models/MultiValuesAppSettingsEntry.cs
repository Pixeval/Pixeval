using System.Collections.Generic;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;
using WinUI3Utilities.Controls;

namespace Pixeval.Settings.Models;

public class MultiValuesAppSettingsEntry(
    AppSettings appSettings,
    string header,
    string description,
    IconGlyph headerIcon,
    IReadOnlyList<SingleValueSettingsEntryBase<AppSettings>> entries)
    : MultiValuesSettingsEntryBase<AppSettings>(appSettings, header, description, headerIcon, entries)
{
    public override MultiValuesAppSettingsExpander Element => new() { Entry = this };
}

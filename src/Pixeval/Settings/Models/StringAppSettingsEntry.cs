using System;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public class StringAppSettingsEntry(
    AppSettings appSettings,
    string propertyName)
    : SingleValueSettingsEntryBase<AppSettings>(appSettings, propertyName)
{
    public override StringSettingsCard Element => new() { Entry = this };

    public string? Value
    {
        get => (string?)ValueBase;
        set => ValueBase = value;
    }

    public Action<string?>? ValueChanged { get; set; }

    public string? Placeholder { get; set; }

    public StringAppSettingsEntry(
        AppSettings appSettings,
        WorkTypeEnum workType,
        string propertyName)
        : this(appSettings, propertyName)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}

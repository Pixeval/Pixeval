using System;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public class IntAppSettingsEntry(
    AppSettings appSettings,
    string propertyName)
    : SingleValueSettingsEntryBase<AppSettings>(appSettings, propertyName)
{
    public override IntSettingsCard Element => new() { Entry = this };

    public int Value
    {
        get => (int)ValueBase!;
        set => ValueBase = value;
    }

    public Action<int>? ValueChanged { get; set; }

    public string? Placeholder { get; set; }

    public double Min { get; set; } = double.NaN;

    public double Max { get; set; } = double.NaN;

    public IntAppSettingsEntry(
        AppSettings appSettings,
        WorkTypeEnum workType,
        string propertyName)
        : this(appSettings, propertyName)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}

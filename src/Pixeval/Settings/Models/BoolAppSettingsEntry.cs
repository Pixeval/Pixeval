using System;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public class BoolAppSettingsEntry(
    AppSettings appSettings,
    string propertyName)
    : SingleValueSettingsEntryBase<AppSettings>(appSettings, propertyName)
{
    public override BoolSettingsCard Element => new() { Entry = this };

    public bool Value
    {
        get => (bool)ValueBase!;
        set => ValueBase = value;
    }

    public Action<bool>? ValueChanged { get; set; }

    public BoolAppSettingsEntry(
        AppSettings appSettings,
        WorkTypeEnum workType,
        string propertyName)
        : this(appSettings, propertyName)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}

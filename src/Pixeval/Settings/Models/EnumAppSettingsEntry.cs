using System;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public class EnumAppSettingsEntry(
    AppSettings appSettings,
    string propertyName,
    Array array)
    : SingleValueSettingsEntryBase<AppSettings>(appSettings, propertyName)
{
    public override EnumSettingsCard Element => new() { Entry = this };

    public Enum Value
    {
        get => (Enum)ValueBase!;
        set => ValueBase = value;
    }

    public Action<Enum>? ValueChanged { get; set; }

    public Array EnumValues { get; set; } = array;

    public EnumAppSettingsEntry(
        AppSettings appSettings,
        WorkTypeEnum workType,
        string propertyName,
        Array array)
        : this(appSettings, propertyName, array)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}

public class EnumAppSettingsEntry<TEnum>(
    AppSettings appSettings,
    string propertyName)
    : EnumAppSettingsEntry(appSettings, propertyName, Enum.GetValues<TEnum>())
    where TEnum : struct, Enum
{
    public EnumAppSettingsEntry(
        AppSettings appSettings,
        WorkTypeEnum workType,
        string propertyName)
        : this(appSettings, propertyName)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}

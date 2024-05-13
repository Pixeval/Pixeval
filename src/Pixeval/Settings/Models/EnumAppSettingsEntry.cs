using System;
using System.Linq.Expressions;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public class EnumAppSettingsEntry(
    AppSettings appSettings,
    Expression<Func<AppSettings, Enum>> property,
    Array array)
    : SingleValueSettingsEntry<AppSettings, Enum>(appSettings, property)
{
    public override FrameworkElement Element => new EnumSettingsCard { Entry = this };

    public Array EnumValues { get; set; } = array;

    public EnumAppSettingsEntry(
        AppSettings appSettings,
        WorkTypeEnum workType,
        Expression<Func<AppSettings, Enum>> property,
        Array array)
        : this(appSettings, property, array)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}

public class EnumAppSettingsEntry<TEnum>(
    AppSettings appSettings,
    Expression<Func<AppSettings, Enum>> property)
    : EnumAppSettingsEntry(appSettings, property, Enum.GetValues<TEnum>())
    where TEnum : struct, Enum
{
    public EnumAppSettingsEntry(
        AppSettings appSettings,
        WorkTypeEnum workType,
        Expression<Func<AppSettings, Enum>> property)
        : this(appSettings, property)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}

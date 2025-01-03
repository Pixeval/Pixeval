using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class EnumAppSettingsEntry(
    SettingsPair<AppSettings> settingsPair,
    Expression<Func<AppSettings, object>> property,
    IReadOnlyList<StringRepresentableItem> array)
    : SingleValueSettingsEntry<AppSettings, object>(settingsPair, property), IEnumSettingsEntry
{
    public override FrameworkElement Element => new EnumSettingsCard { Entry = this };

    public IReadOnlyList<StringRepresentableItem> EnumItems { get; set; } = array;

    public EnumAppSettingsEntry(
        SettingsPair<AppSettings> settingsPair,
        WorkTypeEnum workType,
        Expression<Func<AppSettings, object>> property,
        IReadOnlyList<StringRepresentableItem> array)
        : this(settingsPair, property, array)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}

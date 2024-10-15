using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public class EnumAppSettingsEntry(
    AppSettings appSettings,
    Expression<Func<AppSettings, Enum>> property,
    IReadOnlyList<StringRepresentableItem> array)
    : SingleValueSettingsEntry<AppSettings, Enum>(appSettings, property)
{
    public override FrameworkElement Element => new EnumSettingsCard { Entry = this };

    public IReadOnlyList<StringRepresentableItem> EnumItems { get; set; } = array;

    public EnumAppSettingsEntry(
        AppSettings appSettings,
        WorkTypeEnum workType,
        Expression<Func<AppSettings, Enum>> property,
        IReadOnlyList<StringRepresentableItem> array)
        : this(appSettings, property, array)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}

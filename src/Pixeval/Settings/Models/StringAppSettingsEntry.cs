using System;
using System.Linq.Expressions;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class StringAppSettingsEntry(
    SettingsPair<AppSettings> settingsPair,
    Expression<Func<AppSettings, string>> property)
    : SingleValueSettingsEntry<AppSettings, string>(settingsPair, property), IStringSettingsEntry
{
    public override FrameworkElement Element => new StringSettingsCard { Entry = this };

    public string? Placeholder { get; set; }

    public StringAppSettingsEntry(
        SettingsPair<AppSettings> settingsPair,
        WorkTypeEnum workType,
        Expression<Func<AppSettings, string>> property)
        : this(settingsPair, property)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}

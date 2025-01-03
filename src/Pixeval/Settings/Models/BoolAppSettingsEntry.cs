using System;
using System.Linq.Expressions;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class BoolAppSettingsEntry(
    SettingsPair<AppSettings> settingsPair,
    Expression<Func<AppSettings, bool>> property)
    : SingleValueSettingsEntry<AppSettings, bool>(settingsPair, property)
{
    public override FrameworkElement Element => new BoolSettingsCard { Entry = this };

    public BoolAppSettingsEntry(
        SettingsPair<AppSettings> settingsPair,
        WorkTypeEnum workType,
        Expression<Func<AppSettings, bool>> property)
        : this(settingsPair, property)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}

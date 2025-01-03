using System;
using System.Linq.Expressions;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class BoolAppSettingsEntry(
    AppSettings appSettings,
    Expression<Func<AppSettings, bool>> property)
    : SingleValueSettingsEntry<AppSettings, bool>(appSettings, property)
{
    public override FrameworkElement Element => new BoolSettingsCard { Entry = this };

    public BoolAppSettingsEntry(
        AppSettings appSettings,
        WorkTypeEnum workType,
        Expression<Func<AppSettings, bool>> property)
        : this(appSettings, property)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}

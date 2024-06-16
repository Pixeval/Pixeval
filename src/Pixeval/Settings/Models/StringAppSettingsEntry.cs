using System;
using System.Linq.Expressions;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class StringAppSettingsEntry(
    AppSettings appSettings,
    Expression<Func<AppSettings, string>> property)
    : SingleValueSettingsEntry<AppSettings, string>(appSettings, property)
{
    public override FrameworkElement Element => new StringSettingsCard { Entry = this };

    public string? Placeholder { get; set; }

    public StringAppSettingsEntry(
        AppSettings appSettings,
        WorkTypeEnum workType,
        Expression<Func<AppSettings, string>> property)
        : this(appSettings, property)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}

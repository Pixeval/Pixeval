using System;
using System.Linq.Expressions;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public class BoolAppSettingsEntry(
    AppSettings appSettings,
    Expression<Func<AppSettings, bool>> property)
    : SingleValueSettingsEntry<AppSettings, bool>(appSettings, property)
{
    public override BoolSettingsCard Element => new() { Entry = this };

    public Action<bool>? ValueChanged { get; set; }

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

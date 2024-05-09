using System;
using System.Linq.Expressions;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public class IntAppSettingsEntry(
    AppSettings appSettings,
    Expression<Func<AppSettings, int>> property)
    : SingleValueSettingsEntry<AppSettings, int>(appSettings, property)
{
    public override IntSettingsCard Element => new() { Entry = this };

    public Action<int>? ValueChanged { get; set; }

    public string? Placeholder { get; set; }

    public double Min { get; set; } = double.NaN;

    public double Max { get; set; } = double.NaN;

    public IntAppSettingsEntry(
        AppSettings appSettings,
        WorkTypeEnum workType,
        Expression<Func<AppSettings, int>> property)
        : this(appSettings, property)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}

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

    public string? Placeholder { get; set; }

    public double Max { get; set; } = double.MaxValue;

    public double Min { get; set; } = double.MinValue;

    public double LargeChange { get; set; } = 10;

    public double SmallChange { get; set; } = 1;

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

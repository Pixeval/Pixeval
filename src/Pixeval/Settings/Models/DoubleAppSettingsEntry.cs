// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Linq.Expressions;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class DoubleAppSettingsEntry(SettingsPair<AppSettings> settingsPair, Expression<Func<AppSettings, double>> property)
    : SingleValueSettingsEntry<AppSettings, double>(settingsPair, property), IDoubleSettingsEntry
{
    public override DoubleSettingsCard Element => new() { Entry = this };

    public string? Placeholder { get; set; }

    public double Max { get; set; } = double.MaxValue;

    public double Min { get; set; } = double.MinValue;

    public double LargeChange { get; set; } = 10;

    public double SmallChange { get; set; } = 1;

    public DoubleAppSettingsEntry(
        SettingsPair<AppSettings> settingsPair,
        WorkTypeEnum workType,
        Expression<Func<AppSettings, double>> property)
        : this(settingsPair, property)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}

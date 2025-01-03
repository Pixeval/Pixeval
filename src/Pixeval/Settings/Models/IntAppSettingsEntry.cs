using System;
using System.Linq.Expressions;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class IntAppSettingsEntry(SettingsPair<AppSettings> settingsPair, Expression<Func<AppSettings, int>> property)
    : SingleValueSettingsEntry<AppSettings, int>(settingsPair, property), IDoubleSettingsEntry
{
    public override DoubleSettingsCard Element => new() { Entry = this };

    public string? Placeholder { get; set; }

    public double Max { get; set; } = double.MaxValue;

    public double Min { get; set; } = double.MinValue;

    public double LargeChange { get; set; } = 10;

    public double SmallChange { get; set; } = 1;

    public IntAppSettingsEntry(
        SettingsPair<AppSettings> settingsPair,
        WorkTypeEnum workType,
        Expression<Func<AppSettings, int>> property)
        : this(settingsPair, property)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }

    /// <remarks>
    /// 它和<see cref="SingleValueSettingsEntry{T1, T2}.Value"/>名称相同，所以<see cref="ObservableSettingsEntryBase.OnPropertyChanged"/>只需要触发其中一个就行
    /// </remarks>
    double ISingleValueSettingsEntry<double>.Value
    {
        get => Value;
        set => Value = (int)value;
    }
}

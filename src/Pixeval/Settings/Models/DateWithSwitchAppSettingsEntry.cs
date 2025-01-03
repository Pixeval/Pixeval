using System;
using System.Linq.Expressions;
using Windows.Foundation.Collections;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class DateWithSwitchAppSettingsEntry : BoolAppSettingsEntry
{
    public DateWithSwitchAppSettingsEntry(SettingsPair<AppSettings> settingsPair,
        Expression<Func<AppSettings, bool>> switchProperty,
        Expression<Func<AppSettings, DateTimeOffset>> dateProperty) : base(settingsPair, switchProperty)
    {
        _values = settingsPair.Values;
        (_getter, _setter, var member) = GetSettingsEntryInfo(dateProperty);
        _token = member.Member.Name;
    }

    public override DateWithSwitchSettingsCard Element => new() { Entry = this };

    private readonly Func<AppSettings, DateTimeOffset> _getter;

    private readonly Action<AppSettings, DateTimeOffset> _setter;

    private readonly string _token;

    private readonly IPropertySet _values;

    public Action<DateTimeOffset>? DateChanged { get; set; }

    public DateTimeOffset Date
    {
        get => _getter(Settings);
        set
        {
            if (Equals(Date, value))
                return;
            _setter(Settings, value);
            OnPropertyChanged();
        }
    }

    public override void ValueReset()
    {
        // 顺序很重要，因为base.ValueReset()最后会触发ValueChanged
        OnPropertyChanged(nameof(Date));
        base.ValueReset();
        DateChanged?.Invoke(Date);
    }

    public override void ValueSaving()
    {
        base.ValueSaving();
        _values[_token] = Converter.Convert(Date);
    }
}

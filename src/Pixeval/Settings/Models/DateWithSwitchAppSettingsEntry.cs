// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Linq.Expressions;
using Windows.Foundation.Collections;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class DateWithSwitchAppSettingsEntry : BoolAppSettingsEntry
{
    public DateWithSwitchAppSettingsEntry(AppSettings settings,
        Expression<Func<AppSettings, bool>> switchProperty,
        Expression<Func<AppSettings, DateTimeOffset>> dateProperty) : base(settings, switchProperty)
    {
        (_getter, _setter, var member) = GetSettingsEntryInfo(dateProperty);
        _token = member.Member.Name;
    }

    public override DateWithSwitchSettingsCard Element => new() { Entry = this };

    private readonly Func<AppSettings, DateTimeOffset> _getter;

    private readonly Action<AppSettings, DateTimeOffset> _setter;

    private readonly string _token;

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
            DateChanged?.Invoke(Date);
        }
    }

    public override void ValueReset(AppSettings defaultSettings)
    {
        base.ValueReset(defaultSettings);
        Date = _getter(defaultSettings);
    }

    public override void ValueSaving(IPropertySet values)
    {
        base.ValueSaving(values);
        values[_token] = Converter.Convert(Date);
    }
}

using System;
using System.Linq.Expressions;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class DateWithSwitchAppSettingsEntry : BoolAppSettingsEntry
{
    public DateWithSwitchAppSettingsEntry(AppSettings appSettings,
        Expression<Func<AppSettings, bool>> switchProperty,
        Expression<Func<AppSettings, DateTimeOffset>> dateProperty) : base(appSettings, switchProperty)
    {
        (_getter, _setter, _) = GetSettingsEntryInfo(dateProperty);
    }

    public override DateWithSwitchSettingsCard Element => new() { Entry = this };

    private readonly Func<AppSettings, DateTimeOffset> _getter;

    private readonly Action<AppSettings, DateTimeOffset> _setter;

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
}

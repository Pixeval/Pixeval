using System;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class DateRangeWithSwitchAppSettingsEntry(
    AppSettings appSettings)
    : BoolAppSettingsEntry(appSettings, t => t.UsePreciseRangeForSearch)
{
    public override DateRangeWithSwitchSettingsExpander Element => new() { Entry = this };

    public DateTimeOffset SearchStartDate
    {
        get => Settings.SearchStartDate;
        set
        {
            if (Settings.SearchStartDate != value)
                return;
            Settings.SearchStartDate = value;
            OnPropertyChanged();
        }
    }

    public DateTimeOffset SearchEndDate
    {
        get => Settings.SearchEndDate;
        set
        {
            if (Settings.SearchEndDate != value)
                return;
            Settings.SearchEndDate = value;
            OnPropertyChanged();
        }
    }

    public override void ValueReset()
    {
        base.ValueReset();
        OnPropertyChanged(nameof(SearchStartDate));
        OnPropertyChanged(nameof(SearchEndDate));
    }
}

using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public class DateRangeWithSwitchAppSettingsEntry(
    AppSettings appSettings)
    : ObservableSettingsEntryBase<AppSettings>(appSettings, "", "", default)
{
    public override DateRangeWithSwitchSettingsExpander Element => new() { Entry = this };

    public override void ValueReset() => OnPropertyChanged(nameof(Settings));
}
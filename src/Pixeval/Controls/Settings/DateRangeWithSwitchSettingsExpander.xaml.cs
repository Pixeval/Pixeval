using Microsoft.UI.Xaml;
using Pixeval.Settings.Models;

namespace Pixeval.Controls.Settings;

public sealed partial class DateRangeWithSwitchSettingsExpander
{
    public DateRangeWithSwitchAppSettingsEntry Entry { get; set; } = null!;

    public DateRangeWithSwitchSettingsExpander() => InitializeComponent();

    private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        SettingsCard.IsEnabled = SettingsCard2.IsEnabled = !Entry.Settings.UsePreciseRangeForSearch;
    }
}

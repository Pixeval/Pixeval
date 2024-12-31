using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Settings.Models;
using WinUI3Utilities;

namespace Pixeval.Controls.Settings;

public sealed partial class DateWithSwitchSettingsCard
{
    public DateWithSwitchAppSettingsEntry Entry { get; set; } = null!;

    public DateWithSwitchSettingsCard() => InitializeComponent();

    private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        Entry.ValueChanged?.Invoke(sender.To<ToggleSwitch>().IsOn);
    }
}

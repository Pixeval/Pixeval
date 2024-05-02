using Microsoft.UI.Xaml;
using Pixeval.Settings.Models;

namespace Pixeval.Controls.Settings;

public sealed partial class IpWithSwitchSettingsExpander
{
    public IpWithSwitchAppSettingsEntry Entry { get; set; } = null!;

    public IpWithSwitchSettingsExpander() => InitializeComponent();

    private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        Entry.ValueChanged?.Invoke(Entry.Settings.EnableDomainFronting);
    }
}

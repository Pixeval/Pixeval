using Microsoft.UI.Xaml;

namespace Pixeval.Settings;

public sealed partial class IpWithSwitchSettingsExpander
{
    public IpWithSwitchAppSettingsEntry Entry { get; set; } = null!;

    public IpWithSwitchSettingsExpander() => InitializeComponent();

    private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        Entry.ValueChanged?.Invoke(Entry.Settings.EnableDomainFronting);
    }
}

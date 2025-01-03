using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Settings;
using WinUI3Utilities;

namespace Pixeval.Controls.Settings;

public sealed partial class BoolSettingsCard
{
    public ISingleValueSettingsEntry<bool> Entry { get; set; } = null!;

    public BoolSettingsCard() => InitializeComponent();

    private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        Entry.ValueChanged?.Invoke(sender.To<ToggleSwitch>().IsOn);
    }
}

using Microsoft.UI.Xaml;

namespace Pixeval.Settings;

public sealed partial class BoolSettingsCard
{
    public BoolAppSettingsEntry Entry { get; set; } = null!;

    public BoolSettingsCard() => InitializeComponent();

    private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        Entry.ValueChanged?.Invoke(Entry.Value);
    }
}

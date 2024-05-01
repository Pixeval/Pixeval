using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Settings;

public sealed partial class IntSettingsCard 
{
    public IntAppSettingsEntry Entry { get; set; } = null!;

    public IntSettingsCard() => InitializeComponent();

    private void NumberBox_OnValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs e)
    {
        Entry.ValueChanged?.Invoke(Entry.Value);
    }
}

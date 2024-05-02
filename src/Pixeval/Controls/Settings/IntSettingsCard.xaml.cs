using Microsoft.UI.Xaml.Controls;
using Pixeval.Settings.Models;

namespace Pixeval.Controls.Settings;

public sealed partial class IntSettingsCard 
{
    public IntAppSettingsEntry Entry { get; set; } = null!;

    public IntSettingsCard() => InitializeComponent();

    private void NumberBox_OnValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs e)
    {
        Entry.ValueChanged?.Invoke(Entry.Value);
    }
}

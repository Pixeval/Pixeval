using Microsoft.UI.Xaml.Controls;
using Pixeval.Settings;

namespace Pixeval.Controls.Settings;

public sealed partial class DoubleSettingsCard 
{
    public IDoubleSettingsEntry Entry { get; set; } = null!;

    public DoubleSettingsCard() => InitializeComponent();

    private void NumberBox_OnValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs e)
    {
        Entry.ValueChanged?.Invoke(Entry.Value);
    }
}

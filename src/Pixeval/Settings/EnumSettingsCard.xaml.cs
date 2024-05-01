using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Settings;

public sealed partial class EnumSettingsCard
{
    public EnumAppSettingsEntry Entry { get; set; } = null!;

    public EnumSettingsCard() => InitializeComponent();

    private void EnumComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Entry.ValueChanged?.Invoke(Entry.Value);
    }
}

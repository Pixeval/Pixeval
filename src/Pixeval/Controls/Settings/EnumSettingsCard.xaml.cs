using Microsoft.UI.Xaml.Controls;
using Pixeval.Settings;

namespace Pixeval.Controls.Settings;

public sealed partial class EnumSettingsCard
{
    public IEnumSettingsEntry Entry { get; set; } = null!;

    public EnumSettingsCard() => InitializeComponent();

    private void EnumComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Entry.ValueChanged?.Invoke(Entry.Value);
    }
}

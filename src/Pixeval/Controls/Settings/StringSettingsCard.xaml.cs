using Microsoft.UI.Xaml;
using Pixeval.Settings.Models;

namespace Pixeval.Controls.Settings;

public sealed partial class StringSettingsCard
{
    public StringAppSettingsEntry Entry { get; set; } = null!;

    public StringSettingsCard() => InitializeComponent();

    private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        Entry.ValueChanged?.Invoke(Entry.Value);
    }
}

using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Settings;

public sealed partial class StringSettingsCard
{
    public StringAppSettingsEntry Entry { get; set; } = null!;

    public StringSettingsCard() => InitializeComponent();

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        Entry.ValueChanged?.Invoke(Entry.Value);
    }
}

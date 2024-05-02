using Microsoft.UI.Xaml.Controls;
using Pixeval.Settings.Models;

namespace Pixeval.Controls.Settings;

public sealed partial class StringSettingsCard
{
    public StringAppSettingsEntry Entry { get; set; } = null!;

    public StringSettingsCard() => InitializeComponent();

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        Entry.ValueChanged?.Invoke(Entry.Value);
    }
}

using Microsoft.UI.Xaml;
using Pixeval.Settings.Models;

namespace Pixeval.Controls.Settings;

public sealed partial class MultiValuesAppSettingsExpander
{
    public MultiValuesAppSettingsEntry Entry { get; set; } = null!;

    public MultiValuesAppSettingsExpander() => InitializeComponent();

    private void MultiValuesAppSettingsExpander_OnLoaded(object sender, RoutedEventArgs e)
    {
        foreach (var entry in Entry.Entries)
            Items.Add(entry.Element);
    }
}

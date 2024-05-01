using Microsoft.UI.Xaml;

namespace Pixeval.Settings;

public sealed partial class ClickableSettingsCard 
{
    public ClickableAppSettingsEntry Entry { get; set; } = null!;

    public ClickableSettingsCard() => InitializeComponent();

    private void Clicked(object sender, RoutedEventArgs e) => Entry.Clicked();
}

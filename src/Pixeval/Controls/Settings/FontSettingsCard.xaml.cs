using System;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Settings.Models;
using WinUI3Utilities;

namespace Pixeval.Controls.Settings;

public sealed partial class FontSettingsCard
{
    public FontAppSettingsEntry Entry { get; set; } = null!;

    public FontSettingsCard() => InitializeComponent();

    private async void OpenLinkViaTag_OnClicked(object sender, RoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri(sender.To<FrameworkElement>().GetTag<string>()));
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Entry.ValueChanged?.Invoke(Entry.Value);
    }
}

using System;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using WinUI3Utilities;

namespace Pixeval.Settings;

public sealed partial class FontSettingsCard
{
    public FontAppSettingsEntry Entry { get; set; } = null!;

    public FontSettingsCard() => InitializeComponent();

    private async void OpenLinkViaTag_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri(sender.To<FrameworkElement>().GetTag<string>()));
    }
}

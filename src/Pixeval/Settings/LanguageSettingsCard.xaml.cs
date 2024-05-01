using System;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using WinUI3Utilities;

namespace Pixeval.Settings;

public sealed partial class LanguageSettingsCard 
{
    public LanguageAppSettingsEntry Entry { get; set; } = null!;

    public LanguageSettingsCard() => InitializeComponent();

    private async void OpenLinkViaTag_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri(sender.To<FrameworkElement>().GetTag<string>()));
    }
}

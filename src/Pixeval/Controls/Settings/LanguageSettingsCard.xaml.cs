using System;
using Windows.System;
using Microsoft.UI.Xaml;
using Pixeval.Settings.Models;
using WinUI3Utilities;

namespace Pixeval.Controls.Settings;

public sealed partial class LanguageSettingsCard 
{
    public LanguageAppSettingsEntry Entry { get; set; } = null!;

    public LanguageSettingsCard() => InitializeComponent();

    private async void OpenLinkViaTag_OnClicked(object sender, RoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri(sender.To<FrameworkElement>().GetTag<string>()));
    }
}

// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Microsoft.UI.Xaml;
using Pixeval.Settings.Models;
using Windows.System;
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
}

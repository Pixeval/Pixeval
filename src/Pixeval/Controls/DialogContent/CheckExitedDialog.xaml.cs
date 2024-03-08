using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.AppManagement;
using Windows.System;

namespace Pixeval.Controls.DialogContent;

public sealed partial class CheckExitedDialog : UserControl
{
    public CheckExitedDialog() => InitializeComponent();

    private void ClearConfig_OnTapped(object sender, TappedRoutedEventArgs e) => AppInfo.ClearConfig();

    private void ClearSession_OnTapped(object sender, TappedRoutedEventArgs e) => AppInfo.ClearSession();

    private async void OpenLocalFolder_OnTapped(object sender, TappedRoutedEventArgs e) => await Launcher.LaunchFolderAsync(AppKnownFolders.Local.Self);

    private async void OpenRoamingFolder_OnTapped(object sender, TappedRoutedEventArgs e) => await Launcher.LaunchFolderAsync(AppKnownFolders.Roaming.Self);

    private async void OpenLogFolder_OnTapped(object sender, TappedRoutedEventArgs e) => await Launcher.LaunchFolderAsync(AppKnownFolders.Log.Self);
}

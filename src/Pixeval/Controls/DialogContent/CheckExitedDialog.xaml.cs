// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.AppManagement;
using Windows.System;

namespace Pixeval.Controls.DialogContent;

public sealed partial class CheckExitedDialog : UserControl
{
    public CheckExitedDialog() => InitializeComponent();

    private void ClearConfig_OnClicked(object sender, RoutedEventArgs e) => AppInfo.ClearConfig();

    private void ClearContext_OnClicked(object sender, RoutedEventArgs e) => AppInfo.ClearLoginContext();

    private async void OpenLocalFolder_OnClicked(object sender, RoutedEventArgs e) => await Launcher.LaunchFolderPathAsync(AppKnownFolders.Local.FullPath);

    private async void OpenLogFolder_OnClicked(object sender, RoutedEventArgs e) => await Launcher.LaunchFolderPathAsync(AppKnownFolders.Logs.FullPath);
}

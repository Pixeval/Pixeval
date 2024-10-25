#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/SettingsPage.xaml.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using Windows.System;
using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.Database.Managers;
using Pixeval.Util.UI;
using WinUI3Utilities;

namespace Pixeval.Pages.Misc;

[INotifyPropertyChanged]
public sealed partial class SettingsPage : IScrollViewHost, IDisposable
{
    private SettingsPageViewModel ViewModel { get; set; } = null!;

    private bool _disposed;

    public SettingsPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        ViewModel = new SettingsPageViewModel(HWnd);
    }

    public override void OnPageDeactivated(NavigatingCancelEventArgs e) => Dispose();

    private void SettingsPage_OnUnloaded(object sender, RoutedEventArgs e) => Dispose();

    private void CheckForUpdateButton_OnClicked(object sender, RoutedEventArgs e)
    {
        ViewModel.CheckForUpdate();
    }

    private async void OpenLinkViaTag_OnClicked(object sender, RoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri(sender.To<FrameworkElement>().GetTag<string>()));
    }

    private async void ReleaseNotesHyperlink_OnClicked(object sender, RoutedEventArgs e)
    {
        _ = await this.CreateAcknowledgementAsync(SettingsPageResources.ReleaseNotesHyperlinkButtonContent,
            new ScrollView
            {
                Content = new MarkdownTextBlock
                {
                    Config = new MarkdownConfig(),
                    Text = (sender.To<FrameworkElement>().GetTag<string>() is "Newest"
                        ? AppInfo.AppVersion.NewestAppReleaseModel
                        : AppInfo.AppVersion.CurrentAppReleaseModel)?.ReleaseNote ?? ""
                }
            });
    }

    private async void PerformSignOutButton_OnClicked(object sender, RoutedEventArgs e)
    {
        if (await this.CreateOkCancelAsync(SettingsPageResources.SignOutConfirmationDialogTitle,
                SettingsPageResources.SignOutConfirmationDialogContent) is ContentDialogResult.Primary)
        {
            App.AppViewModel.LoginContext.LogoutExit = true;
            // Close 不触发 Closing 事件
            AppInfo.SaveContextWhenExit();
            WindowFactory.RootWindow.Close();
        }
    }

    private async void ResetDefaultSettings_OnClicked(object sender, RoutedEventArgs e)
    {
        if (await this.CreateOkCancelAsync(SettingsPageResources.ResetSettingConfirmationDialogTitle,
                SettingsPageResources.ResetSettingConfirmationDialogContent) is ContentDialogResult.Primary)
        {
            ViewModel.AppSettings.ResetDefault();
            foreach (var simpleSettingsGroup in ViewModel.Groups)
                foreach (var settingsEntry in simpleSettingsGroup)
                    settingsEntry.ValueReset();
            OnPropertyChanged(nameof(ViewModel));
        }
    }

    private void DeleteFileCacheEntryButton_OnClicked(object sender, RoutedEventArgs e)
    {
        _ = AppKnownFolders.Cache.ClearAsync();
        ViewModel.ShowClearData(ClearDataKind.FileCache);
    }

    private void DeleteSearchHistoriesButton_OnClicked(object sender, RoutedEventArgs e)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<SearchHistoryPersistentManager>();
        manager.Clear();
        ViewModel.ShowClearData(ClearDataKind.SearchHistory);
    }

    private void DeleteBrowseHistoriesButton_OnClicked(object sender, RoutedEventArgs e)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        manager.Clear();
        ViewModel.ShowClearData(ClearDataKind.BrowseHistory);
    }

    private void DeleteDownloadHistoriesButton_OnClicked(object sender, RoutedEventArgs e)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        manager.Clear();
        ViewModel.ShowClearData(ClearDataKind.DownloadHistory);
    }

    private void OpenFolder_OnClicked(object sender, RoutedEventArgs e)
    {
        var folder = sender.To<FrameworkElement>().GetTag<string>() switch
        {
            "Log" => AppKnownFolders.Log.Self,
            "Temp" => AppKnownFolders.Temporary.Self,
            "Local" => AppKnownFolders.Local.Self,
            _ => null
        };
        if (folder is not null)
            _ = Launcher.LaunchFolderAsync(folder);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        Bindings.StopTracking();
        foreach (var simpleSettingsGroup in ViewModel.Groups)
            foreach (var settingsEntry in simpleSettingsGroup)
                settingsEntry.ValueSaving();
        AppInfo.SaveConfig(ViewModel.AppSettings);
        ViewModel.Dispose();
        ViewModel = null!;
    }

    /// <summary>
    /// <see cref="ItemsControl"/>会有缓动动画，<see cref="ItemsRepeater"/>会延迟加载，
    /// 使用只好手动一次全部加载，以方便根据Tag导航
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Panel_OnLoaded(object sender, RoutedEventArgs e)
    {
        var panel = sender.To<StackPanel>();
        var style = Resources["SettingHeaderStyle"] as Style;

        foreach (var group in ViewModel.Groups)
        {
            panel.Children.Add(new TextBlock
            {
                Style = style,
                Text = group.Header
            });
            foreach (var entry in group)
                panel.Children.Add(entry.Element);
        }
    }

    public ScrollView ScrollView => SettingsPageScrollView;
}

// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.System;
using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.Controls.Windowing;
using Pixeval.Database.Managers;
using Pixeval.Settings;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;
using CommunityToolkit.WinUI.Controls;
using CommunityToolkit.WinUI;
using Windows.Foundation;

namespace Pixeval.Pages.Misc;

/// <summary>
/// todo INotifyPropertyChanged
/// </summary>
public sealed partial class SettingsPage : IDisposable, INotifyPropertyChanged
{
    private string CurrentVersion =>
        AppInfo.AppVersion.CurrentVersion.Let(t => $"{t.Major}.{t.Minor}.{t.Build}.{t.Revision}");

    private SettingsPageViewModel ViewModel { get; set; } = null!;

    private SettingsEntryAttribute? _scrollToAttribute;

    private bool _disposed;

    public SettingsPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        ViewModel = new SettingsPageViewModel(this);
        _scrollToAttribute = parameter as SettingsEntryAttribute;

        // ItemsControl会有缓动动画，ItemsRepeater会延迟加载，使用只好手动一次全部加载，以方便根据Tag导航
        var style = Resources["SettingHeaderStyle"] as Style;

        foreach (var group in ViewModel.LocalGroups.Concat<ISettingsGroup>(ViewModel.ExtensionGroups))
        {
            SettingsPanel.Children.Add(new TextBlock
            {
                Style = style,
                Text = group.Header
            });
            foreach (var entry in group)
                SettingsPanel.Children.Add(entry.Element);
        }

        var frameworkElement = SettingsPanel.FindChild<SettingsCard>(element => element.Tag is SettingsEntryAttribute a && Equals(a, _scrollToAttribute));

        if (frameworkElement is not null)
        {
            var position = frameworkElement
                .TransformToVisual(SettingsPanel)
                // 神秘的偏移量
                .TransformPoint(new Point(0, -160));

            _ = SettingsPageScrollView.ScrollTo(position.X, position.Y);
        }
    }

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
            var settings = new AppSettings();
            foreach (var localGroup in ViewModel.LocalGroups)
                foreach (var settingsEntry in localGroup)
                    settingsEntry.ValueReset(settings);
            foreach (var extensionGroup in ViewModel.ExtensionGroups)
                foreach (var settingsEntry in extensionGroup)
                    settingsEntry.ValueReset();
            OnPropertyChanged(nameof(ViewModel));
        }
    }

    private void DeleteFileCacheEntryButton_OnClicked(object sender, RoutedEventArgs e)
    {
        AppKnownFolders.Cache.Clear();
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

    private async void OpenFolder_OnClicked(object sender, RoutedEventArgs e)
    {
        var folder = sender.To<FrameworkElement>().GetTag<string>() switch
        {
            nameof(AppKnownFolders.Logs) => AppKnownFolders.Logs,
            nameof(AppKnownFolders.Temp) => AppKnownFolders.Temp,
            nameof(AppKnownFolders.Local) => AppKnownFolders.Local,
            nameof(AppKnownFolders.Extensions) => AppKnownFolders.Extensions,
            _ => null
        };
        if (folder is not null)
            _ = await Launcher.LaunchFolderPathAsync(folder.FullPath);
    }

    /// <summary>
    /// 每次离开页面都进行保存
    /// </summary>
    private void SettingsPage_OnUnloaded(object sender, RoutedEventArgs e)
    {
        foreach (var localGroup in ViewModel.LocalGroups)
        foreach (var settingsEntry in localGroup)
            settingsEntry.ValueSaving(AppInfo.LocalConfig);
        foreach (var extensionGroup in ViewModel.ExtensionGroups)
        foreach (var settingsEntry in extensionGroup)
            settingsEntry.ValueSaving(extensionGroup.Model.Values);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        Bindings.StopTracking();
        ViewModel.Dispose();
        ViewModel = null!;
    }

    public override void CompleteDisposal()
    {
        base.CompleteDisposal();
        Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

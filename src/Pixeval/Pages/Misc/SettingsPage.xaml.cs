// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.Database.Managers;
using Pixeval.Settings;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Windows.Foundation;
using Windows.System;
using WinUI3Utilities;

namespace Pixeval.Pages.Misc;

public sealed partial class SettingsPage : IDisposable
{
    private string CurrentVersion =>
        AppInfo.AppVersion.CurrentVersion.Let(t => $"{t.Major}.{t.Minor}.{t.Build}.{t.Revision}");

    private SettingsPageViewModel _viewModel = null!;

    public SettingsEntryAttribute? TargetAttribute { get; set; }

    private bool _disposed;

    public SettingsPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        _viewModel = new SettingsPageViewModel(this);

        // ItemsControl会有缓动动画，ItemsRepeater会延迟加载，使用只好手动一次全部加载，以方便根据Tag导航
        var style = Resources["SettingHeaderStyle"] as Style;

        foreach (var group in _viewModel.LocalGroups.Concat<ISettingsGroup>(_viewModel.ExtensionGroups))
        {
            SettingsPanel.Children.Add(new TextBlock
            {
                Style = style,
                Text = group.Header
            });
            foreach (var entry in group)
                SettingsPanel.Children.Add(entry.Element);
        }

        ScrollToAttribute(parameter as SettingsEntryAttribute, -135);
    }

    private void SettingsPage_OnLoaded(object sender, RoutedEventArgs e) => ScrollToAttribute(TargetAttribute);

    public void ScrollToAttribute(SettingsEntryAttribute? attribute, double offset = 0)
    {
        if (attribute is null)
            return;

        var frameworkElement = SettingsPanel.FindChild<SettingsCard>(element => element.Tag is SettingsEntryAttribute a && Equals(a, attribute));

        if (frameworkElement is not null)
        {
            var position = frameworkElement
                .TransformToVisual(SettingsPanel)
                // 神秘的偏移量
                .TransformPoint(new Point(0, offset));

            _ = SettingsPageScrollView.ScrollTo(position.X, position.Y);
        }
    }

    private void CheckForUpdateButton_OnClicked(object sender, RoutedEventArgs e)
    {
        _viewModel.CheckForUpdate();
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
        if (await this.CreateOkCancelAsync(ExitDialogResources.SignOutConfirmationDialogTitle,
                ExitDialogResources.SignOutConfirmationDialogContent) is ContentDialogResult.Primary)
        {
            App.AppViewModel.LoginContext.LogoutExit = true;
            WindowFactory.RootWindow.Close();
        }
    }

    private async void ResetDefaultSettings_OnClicked(object sender, RoutedEventArgs e)
    {
        if (await this.CreateOkCancelAsync(SettingsPageResources.ResetSettingConfirmationDialogTitle,
                SettingsPageResources.ResetSettingConfirmationDialogContent) is ContentDialogResult.Primary)
        {
            var settings = new AppSettings();
            foreach (var localGroup in _viewModel.LocalGroups)
                foreach (var settingsEntry in localGroup)
                    settingsEntry.ValueReset(settings);
            foreach (var extensionGroup in _viewModel.ExtensionGroups)
                foreach (var settingsEntry in extensionGroup)
                    settingsEntry.ValueReset();
        }
    }

    private void DeleteFileCacheEntryButton_OnClicked(object sender, RoutedEventArgs e)
    {
        AppKnownFolders.Cache.Clear();
        _viewModel.ShowClearData(ClearDataKind.FileCache);
    }

    private void DeleteSearchHistoriesButton_OnClicked(object sender, RoutedEventArgs e)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<SearchHistoryPersistentManager>();
        manager.Clear();
        _viewModel.ShowClearData(ClearDataKind.SearchHistory);
    }

    private void DeleteBrowseHistoriesButton_OnClicked(object sender, RoutedEventArgs e)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        manager.Clear();
        _viewModel.ShowClearData(ClearDataKind.BrowseHistory);
    }

    private void DeleteDownloadHistoriesButton_OnClicked(object sender, RoutedEventArgs e)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        manager.Clear();
        _viewModel.ShowClearData(ClearDataKind.DownloadHistory);
    }

    private async void OpenFolder_OnClicked(object sender, RoutedEventArgs e)
    {
        var folder = sender.To<FrameworkElement>().GetTag<string>() switch
        {
            nameof(AppKnownFolders.Logs) => AppKnownFolders.Logs,
            nameof(AppKnownFolders.Temp) => AppKnownFolders.Temp,
            nameof(AppKnownFolders.Local) => AppKnownFolders.Local,
            nameof(AppKnownFolders.Extensions) => AppKnownFolders.Extensions,
            nameof(AppKnownFolders.Settings) => AppKnownFolders.Settings,
            _ => null
        };
        if (folder is not null)
            _ = await Launcher.LaunchFolderPathAsync(folder.FullPath);
    }

    /// <summary>
    /// 每次离开页面都进行保存
    /// </summary>
    private void SettingsPage_OnUnloaded(object sender, RoutedEventArgs e) => ValueSaving();

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        Bindings.StopTracking();
        ValueSaving();
        _viewModel.Dispose();
        _viewModel = null!;
    }

    public override void CompleteDisposal()
    {
        base.CompleteDisposal();
        Dispose();
    }

    /// <summary>
    /// 保存设置
    /// </summary>
    private void ValueSaving()
    {
        if (_viewModel == null!)
            return;
        foreach (var localGroup in _viewModel.LocalGroups)
            foreach (var settingsEntry in localGroup)
                settingsEntry.ValueSaving(AppInfo.LocalConfig);
        foreach (var extensionGroup in _viewModel.ExtensionGroups)
            foreach (var settingsEntry in extensionGroup)
                settingsEntry.ValueSaving(extensionGroup.Model.Values);
    }

    private async void ExportSettingsPlainText_OnClicked(object sender, RoutedEventArgs e)
    {
        if (await this.OpenFolderPickerAsync() is not { } folder)
            return;
        try
        {
            var jsonSettingsPath = Path.Combine(folder.Path, Path.ChangeExtension(AppKnownFolders.SettingsDatName, "json"));
            var jsonSessionPath = Path.Combine(folder.Path, "session.json");
            await using var jsonSettingsStream = IoHelper.CreateAsyncWrite(jsonSettingsPath);
            await using var jsonSessionStream = IoHelper.CreateAsyncWrite(jsonSessionPath);
            await JsonSerializer.SerializeAsync(jsonSettingsStream, _viewModel.AppSettings, typeof(AppSettings), SettingsSerializeContext.Default);
            await JsonSerializer.SerializeAsync(jsonSessionStream, App.AppViewModel.LoginContext, typeof(LoginContext), SettingsSerializeContext.Default);

            this.SuccessGrowl(SettingsPageResources.ExportSettingsSuccess);
        }
        catch (Exception exception)
        {
            this.ErrorGrowl(exception.Message, exception.StackTrace);
        }
    }

    private async void ImportSettingsPlaintext_OnClicked(object sender, RoutedEventArgs e)
    {
        if (await this.OpenMultipleJsonsOpenPickerAsync() is not { } files)
            return;
        try
        {
            foreach (var file in files)
            {
                var path = file.Path;
                await using var stream = IoHelper.OpenAsyncRead(path);
                switch (file.Name)
                {
                    case "session.json":
                    {
                        if (await JsonSerializer.DeserializeAsync(stream, typeof(LoginContext), SettingsSerializeContext.Default) is LoginContext loginContext)
                        {
                            loginContext.CopyTo(App.AppViewModel.LoginContext);
                            this.SuccessGrowl(SettingsPageResources.ImportSessionSuccess, file.Name);
                        }
                        break;
                    }
                    case "settings.json":
                    {
                        if (await JsonSerializer.DeserializeAsync(stream, typeof(AppSettings), SettingsSerializeContext.Default) is AppSettings appSettings)
                        {
                            foreach (var localGroup in _viewModel.LocalGroups)
                                foreach (var settingsEntry in localGroup)
                                    settingsEntry.ValueReset(appSettings);
                            this.SuccessGrowl(SettingsPageResources.ImportSettingsSuccess, file.Name);
                        }
                        break;
                    }
                }
            }
        }
        catch (Exception exception)
        {
            this.ErrorGrowl(exception.Message, exception.StackTrace);
        }
    }
}

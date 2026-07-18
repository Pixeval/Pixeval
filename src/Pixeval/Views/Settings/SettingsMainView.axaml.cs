// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using AutoSettingsPage.Models;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Models.Navigation;
using Pixeval.Models.Options;
using Pixeval.Models.Settings;
using Pixeval.Utilities;
using Pixeval.Utilities.IO.Caching;
using Pixeval.ViewModels;
using Pixeval.Views.Login;
using SharpYaml;

namespace Pixeval.Views.Settings;

public partial class SettingsMainView : ContentPage
{
    public SettingsMainView()
    {
        InitializeComponent();
        RefreshUpdateStatus();
    }

    private async void SettingsCard_OnClick(object? sender, RoutedEventArgs e)
    {
        if (IsInNavigationPage && Parent is NavigationPage frame && sender is Control { DataContext: ISettingsGroup group })
            await frame.PushAsync(new SettingsSubView(group));
    }

    private void SwitchAccountButton_OnClicked(object sender, RoutedEventArgs e)
    {
        TopLevel.GetTopLevel(this)?.ViewContainer?.NavigateTo(new LoginPage());
    }

    private async void ResetDefaultSettings_OnClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsPageViewModel vm)
            return;

        if (TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;

        if (await viewContainer.CreateOkCancelAsync(
                I18NManager.GetResource(SettingsMainViewResources.ResetSettingConfirmationDialogTitle),
                I18NManager.GetResource(SettingsMainViewResources.ResetSettingConfirmationDialogContent))
            is not ContentDialogResult.Primary)
            return;

        var settings = new AppSettings();
        App.AppViewModel.NavigationMenuYamlText = NavigationMenuYaml.DefaultYaml;
        App.AppViewModel.ResetHomePageCards();
        foreach (var localGroup in vm.LocalGroups)
            foreach (var settingsEntry in localGroup)
                settingsEntry.LocalValueReset(settings);
        foreach (var extensionGroup in vm.ExtensionGroups)
            foreach (var settingsEntry in extensionGroup)
                settingsEntry.ValueReset();

        if (viewContainer is ViewContainers.TabViewContainer container)
            container.ReloadNavigation();
    }

    private async void ReleaseNotesHyperlink_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer)
            _ = await viewContainer.CreateAcknowledgementTaskAsync(
                I18NManager.GetResource(SettingsMainViewResources.ReleaseNoteDialogTitle),
                async contentDialog =>
                {
                    var control = await SettingsPage.GetReleaseNotesAsync();
                    RefreshUpdateStatus();
                    contentDialog.Title = SettingsPage.ReleaseTitle;
                    contentDialog.Content = control;
                });
    }

    private async void CheckForUpdateButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        button.IsEnabled = false;
        UpdateStatusInfoBar.Mode = InfoBarMode.Info;
        UpdateStatusInfoBar.Text = I18NManager.GetResource(SettingsMainViewResources.CheckingForUpdate);
        try
        {
            await AppInfo.AppVersion.GitHubCheckForUpdateAsync();
            RefreshUpdateStatus();
        }
        finally
        {
            button.IsEnabled = true;
        }

        if (AppInfo.AppVersion is not { UpdateAvailable: true, NewestAppReleaseModel: { } release }
            || TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;

        _ = await viewContainer.CreateAcknowledgementAsync(
            SettingsPage.GetReleaseTitle(release.Version),
            SettingsPage.CreateReleaseNotes(release));
    }

    private void RefreshUpdateStatus()
    {
        if (DataContext is SettingsPageViewModel vm)
            vm.RefreshLastCheckedUpdate();

        var updateState = AppInfo.AppVersion.UpdateState;
        var (mode, resource) = updateState switch
        {
            UpdateState.UpToDate => (InfoBarMode.Success, SettingsMainViewResources.IsUpToDate),
            UpdateState.MajorUpdate => (InfoBarMode.Warning, SettingsMainViewResources.UpdateAvailableMajor),
            UpdateState.MinorUpdate => (InfoBarMode.Warning, SettingsMainViewResources.UpdateAvailableMinor),
            UpdateState.BuildUpdate => (InfoBarMode.Warning, SettingsMainViewResources.UpdateAvailableBuild),
            UpdateState.Insider => (InfoBarMode.Info, SettingsMainViewResources.IsInsider),
            UpdateState.Unknown => (InfoBarMode.Info, SettingsMainViewResources.UpdateAvailableUnknown),
            _ => throw new ArgumentOutOfRangeException(nameof(updateState), updateState, null)
        };
        UpdateStatusInfoBar.Mode = mode;
        UpdateStatusInfoBar.Text = I18NManager.GetResource(resource);
    }

    private async void ExportSettingsPlainText_OnClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsPageViewModel vm
            || TopLevel.GetTopLevel(this) is not { ViewContainer: { } viewContainer, StorageProvider: { } provider })
            return;
        try
        {
            if (await provider.SaveFilePickerAsync(new()
                {
                    FileTypeChoices =
                    [
                        new("YAML")
                        {
                            Patterns = ["*.yaml"],
                            MimeTypes = ["text/yaml", "application/x-yaml"]
                        }
                    ],
                    DefaultExtension = "yaml",
                    SuggestedFileName = "settings"
                }) is not { } file)
                return;
            await using var stream = await file.OpenWriteAsync();
            YamlSerializer.Serialize(stream, vm.AppSettings, SettingsSerializerContext.Default.AppSettings);

            viewContainer.ShowSuccess(I18NManager.GetResource(SettingsMainViewResources.ExportSettingsSuccess));
        }
        catch (Exception exception)
        {
            viewContainer.ShowError(exception.Message, exception.StackTrace);
        }
    }

    private async void ImportSettingsPlaintext_OnClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsPageViewModel vm
            || TopLevel.GetTopLevel(this) is not { ViewContainer: { } viewContainer, StorageProvider: { } provider })
            return;
        try
        {
            if (await provider.OpenFilePickerAsync(new()
                {
                    FileTypeFilter = [new("YAML") { Patterns = ["*.yaml"] }],
                    AllowMultiple = false
                }) is not [{ } file])
                return;

            await using var stream = await file.OpenReadAsync();

            if (YamlSerializer.Deserialize(stream, SettingsSerializerContext.Default.AppSettings) is { } appSettings)
            {
                foreach (var localGroup in vm.LocalGroups)
                    foreach (var settingsEntry in localGroup)
                        settingsEntry.LocalValueReset(appSettings);
                viewContainer.ShowSuccess(I18NManager.GetResource(SettingsMainViewResources.ImportSettingsSuccess), file.Name);
            }
        }
        catch (Exception exception)
        {
            viewContainer.ShowError(exception.Message, exception.StackTrace);
        }
    }

    private void DeleteFileCacheEntryButton_OnClicked(object sender, RoutedEventArgs e)
    {
        CacheHelper.PurgeCache();
        ShowClearData(ClearDataKind.FileCache);
    }

    private void DeleteSearchHistoriesButton_OnClicked(object sender, RoutedEventArgs e)
    {
        App.AppViewModel.HistoryPersistHelper.SearchHistoryEntries.Clear();
        ShowClearData(ClearDataKind.SearchHistory);
    }

    private void DeleteBrowseHistoriesButton_OnClicked(object sender, RoutedEventArgs e)
    {
        App.AppViewModel.HistoryPersistHelper.ClearBrowseHistory();
        ShowClearData(ClearDataKind.BrowseHistory);
    }

    private void DeleteDownloadHistoriesButton_OnClicked(object sender, RoutedEventArgs e)
    {
        App.AppViewModel.HistoryPersistHelper.DownloadManager.ClearTasks();
        ShowClearData(ClearDataKind.DownloadHistory);
    }

    public void ShowClearData(ClearDataKind kind)
    {
        if (TopLevel.GetTopLevel(this) is not { ViewContainer: { } viewContainer })
            return;
        viewContainer.ShowSuccess(I18NManager.GetResource(kind switch
        {
            ClearDataKind.FileCache => EnumResources.ClearDataKindFileCache,
            ClearDataKind.BrowseHistory => EnumResources.ClearDataKindBrowseHistory,
            ClearDataKind.SearchHistory => EnumResources.ClearDataKindSearchHistory,
            ClearDataKind.DownloadHistory => EnumResources.ClearDataKindDownloadHistory,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        }));
    }

    private async void AboutPageButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (IsInNavigationPage && Parent is NavigationPage frame)
            await frame.PushAsync(new AboutPage());
    }

    private async void HelpPageButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (IsInNavigationPage && Parent is NavigationPage frame)
            await frame.PushAsync(new HelpPage());
    }

    private async void NavigationSettingsButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (IsInNavigationPage && Parent is NavigationPage frame)
            await frame.PushAsync(new NavigationSettingsPage());
    }
}

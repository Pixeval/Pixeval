using System;
using System.IO;
using System.Text.Json;
using AutoSettingsPage.Models;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.AppManagement;
using Pixeval.I18N;
using Pixeval.Models.Database.Managers;
using Pixeval.Models.Options;
using Pixeval.Models.Settings;
using Pixeval.Utilities;
using Pixeval.ViewModels;

namespace Pixeval.Views.Settings;

public partial class SettingsMainView : ContentPage
{
    public SettingsMainView()
    {
        InitializeComponent();
    }

    private async void SettingsCard_OnClick(object? sender, RoutedEventArgs e)
    {
        if (IsInNavigationPage && Parent is NavigationPage frame && sender is Control { DataContext: ISettingsGroup group })
            await frame.PushAsync(new SettingsSubView(group));
    }

    private async void PerformSignOutButton_OnClicked(object sender, RoutedEventArgs e)
    {
        //if (await this.CreateOkCancelAsync(SettingsPageResources.ResetSettingConfirmationDialogTitle,
        //        SettingsPageResources.ResetSettingConfirmationDialogContent) is ContentDialogResult.Primary)
        // 关闭除此之外所有窗口
        TopLevel.GetTopLevel(this)?.ViewContainer?.NavigateTo<Login.LoginPage>();
    }

    private async void ResetDefaultSettings_OnClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsPageViewModel vm)
            return;

        //if (await this.CreateOkCancelAsync(SettingsPageResources.ResetSettingConfirmationDialogTitle,
        //        SettingsPageResources.ResetSettingConfirmationDialogContent) is ContentDialogResult.Primary)
        {
            var settings = new AppSettings();
            foreach (var localGroup in vm.LocalGroups)
            foreach (var settingsEntry in localGroup)
                settingsEntry.LocalValueReset(settings);
            foreach (var extensionGroup in vm.ExtensionGroups)
            foreach (var settingsEntry in extensionGroup)
                settingsEntry.ValueReset();
        }
    }

    private void ReleaseNotesHyperlink_OnClicked(object? sender, RoutedEventArgs e)
    {
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
                    DefaultExtension = "json",
                    SuggestedFileName = "settings"
                }) is not { } file)
                return;
            await using var stream = await file.OpenWriteAsync();
            await JsonSerializer.SerializeAsync(stream, vm.AppSettings, SettingsSerializerContext.Default.AppSettings);

            viewContainer.ShowSuccess(I18NManager.GetResource(SettingsPageResources.ExportSettingsSuccess));
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
                    FileTypeFilter = [new("JSON") { Patterns = ["*.json"] }],
                    AllowMultiple = false
                }) is not [{ } file])
                return;

            await using var stream = await file.OpenReadAsync();
            //if (await JsonSerializer.DeserializeAsync(stream, SettingsSerializerContext.Default.LoginContext) is { } loginContext)
            //{
            //    loginContext.CopyTo(App.AppViewModel.LoginContext);
            //    viewContainer.ShowSuccess(SettingsPageResources.ImportSessionSuccess, file.Name);
            //}
            if (await JsonSerializer.DeserializeAsync(stream, SettingsSerializerContext.Default.AppSettings) is { } appSettings)
            {
                foreach (var localGroup in vm.LocalGroups)
                foreach (var settingsEntry in localGroup)
                    settingsEntry.LocalValueReset(appSettings);
                viewContainer.ShowSuccess(I18NManager.GetResource(SettingsPageResources.ImportSettingsSuccess), file.Name);
            }
        }
        catch (Exception exception)
        {
            viewContainer.ShowError(exception.Message, exception.StackTrace);
        }
    }

    private void DeleteFileCacheEntryButton_OnClicked(object sender, RoutedEventArgs e)
    {
        Directory.Delete(AppInfo.CacheFolder, true);
        _ = Directory.CreateDirectory(AppInfo.CacheFolder);
        ShowClearData(ClearDataKind.FileCache);
    }

    private void DeleteSearchHistoriesButton_OnClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsPageViewModel vm)
            return;
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<SearchHistoryPersistentManager>();
        manager.Clear();
        ShowClearData(ClearDataKind.SearchHistory);
    }

    private void DeleteBrowseHistoriesButton_OnClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsPageViewModel vm)
            return;
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        manager.Clear();
        ShowClearData(ClearDataKind.BrowseHistory);
    }

    private void DeleteDownloadHistoriesButton_OnClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsPageViewModel vm)
            return;
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        manager.Clear();
        ShowClearData(ClearDataKind.DownloadHistory);
    }

    public void ShowClearData(ClearDataKind kind)
    {
        if (TopLevel.GetTopLevel(this) is not { ViewContainer: { } viewContainer })
            return;
        viewContainer.ShowSuccess(I18NManager.GetResource(kind));
    }
}

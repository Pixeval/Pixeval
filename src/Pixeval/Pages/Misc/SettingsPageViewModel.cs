// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using AutoSettingsPage;
using AutoSettingsPage.Models;
using AutoSettingsPage.WinUI;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Global.Enum;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.Extensions;
using Pixeval.Options;
using Pixeval.Settings;
using Pixeval.Util.ComponentModels;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Pages.Misc;

public partial class SettingsPageViewModel : UiObservableObject, IDisposable
{
    public DateTimeOffset LastCheckedUpdate
    {
        get => AppSettings.LastCheckedUpdate;
        set => SetProperty(AppSettings.LastCheckedUpdate, value, AppSettings, (setting, v) => AppInfo.LocalConfig[nameof(AppSettings.LastCheckedUpdate)] = setting.LastCheckedUpdate = v);
    }

    public bool DownloadUpdateAutomatically
    {
        get => AppSettings.DownloadUpdateAutomatically;
        set => SetProperty(AppSettings.DownloadUpdateAutomatically, value, AppSettings, (setting, v) => AppInfo.LocalConfig[nameof(AppSettings.DownloadUpdateAutomatically)] = setting.DownloadUpdateAutomatically = v);
    }

    public AppSettings AppSettings => App.AppViewModel.AppSettings;

    [ObservableProperty]
    public partial bool CheckingUpdate { get; set; }

    [ObservableProperty]
    public partial bool DownloadingUpdate { get; set; }

    [ObservableProperty]
    public partial double DownloadingUpdateProgress { get; set; }

    [ObservableProperty]
    public partial string? UpdateMessage { get; set; }

    [ObservableProperty]
    public partial bool ExpandExpander { get; set; }

    private CancellationTokenSource? _cancellationTokenSource;

    /// <inheritdoc/>
    public SettingsPageViewModel(FrameworkElement frameworkElement) : base(frameworkElement)
    {
        LocalGroups = SettingsBuilder.CreateGroupList(AppSettings)
            .NewGroup(SettingsEntryCategory.Application)
            .Config(group => group
                .Enum(t => t.Theme, ElementTheme.Pairs,
                    entry => entry.ValueChanged += t => WindowFactory.SetTheme((ElementTheme) t))
                .Enum(t => t.Backdrop, BackdropType.Pairs,
                    entry => entry.ValueChanged += t => WindowFactory.SetBackdrop((BackdropType) t))
                .Font(t => t.AppFontFamilyName)
                .Language()
                .DomainFronting(t => t.EnableDomainFronting, entry =>
                        entry.IPSet(t => t.PixivAppApiNameResolver)
                            .IPSet(t => t.PixivImageNameResolver)
                            .IPSet(t => t.PixivImageNameResolver2)
                            .IPSet(t => t.PixivOAuthNameResolver)
                            .IPSet(t => t.PixivAccountNameResolver)
                            .IPSet(t => t.PixivWebApiNameResolver),
                    entry => entry.ValueChanged += t => App.AppViewModel.MakoClient.Configuration.DomainFronting = t)
                .Proxy(entry => entry.ProxyChanged = t => App.AppViewModel.MakoClient.Configuration.Proxy = t)
                .String(t => t.MirrorHost,
                    entry => entry.ValueChanged += t => App.AppViewModel.MakoClient.Configuration.MirrorHost = t)
                .Bool(t => t.UseFileCache)
                .Enum(t => t.DefaultSelectedTabItem, MainPageTabItem.Pairs)
                .String(t => t.WebCookie,
                    entry => entry.ValueChanged += t => App.AppViewModel.MakoClient.Configuration.Cookie = t)
                // Min=48时NavigationViewItem的图标会被挡住一部分
                .Int(t => t.NavigationViewOpenPaneWidth, 50, 1000, 10,
                    entry => entry.ValueChanged += t => MainPage.Current.NavigationView.OpenPaneLength = t)
                .Bool(t => t.ReconfirmationOfClosingWindow))
            .NewGroup(SettingsEntryCategory.BrowsingExperience)
            .Config(group => group
                .Enum(t => t.ThumbnailDirection, ThumbnailDirection.Pairs)
                .Enum(t => t.ItemsViewLayoutType, ItemsViewLayoutType.Pairs)
                .Enum(t => t.TargetFilter, TargetFilter.Pairs)
                .Collection(t => t.BlockedTags)
                .Bool(t => t.BrowseOriginalImage)
                .Double(t => t.ScrollRate, 0, 10, 1,
                    entry => entry.ValueChanged += d => AdvancedItemsView.ScrollRate = (float) d)
                .Bool(t => t.OpenWorkInfoByDefault))
            .NewGroup(SettingsEntryCategory.Search)
            .Config(group => group
                .String(t => t.ReverseSearchApiKey,
                    entry => entry.DescriptionUri = new("https://saucenao.com/user.php?page=search-api"))
                .Int(t => t.ReverseSearchResultSimilarityThreshold, 1, 100, 1)
                .Int(t => t.MaximumSearchHistoryRecords, 10, 200, 1)
                .Int(t => t.MaximumSuggestionBoxSearchHistory, 0, 20, 1)
                .Enum(t => t.WorkSortOption, WorkSortOption.Pairs)
                .Enum(t => t.SimpleWorkType, SimpleWorkType.Pairs)
                .MultiValues(t => t.IllustrationRankOption, entry =>
                    entry.Enum(
                            WorkTypeEnum.Illustration,
                            t => t.IllustrationRankOption,
                            RankOption.IllustrationPairs)
                        .Enum(
                            WorkTypeEnum.Novel,
                            t => t.NovelRankOption,
                            RankOption.NovelPairs))
                .MultiValues(t => t.SearchIllustrationTagMatchOption, entry =>
                    entry.Enum(
                            WorkTypeEnum.Illustration,
                            t => t.SearchIllustrationTagMatchOption,
                            SearchIllustrationTagMatchOption.Pairs)
                        .Enum(
                            WorkTypeEnum.Novel,
                            t => t.SearchNovelTagMatchOption,
                            SearchNovelTagMatchOption.Pairs))
                .DateWithSwitch(t => t.UseSearchStartDate,
                    entry => entry.DateTimeOffset(t => t.SearchStartDate, DateTimeOffset.MinValue,
                        DateTimeOffset.MaxValue))
                .DateWithSwitch(t => t.UseSearchEndDate,
                    entry => entry.DateTimeOffset(t => t.SearchEndDate, DateTimeOffset.MinValue,
                        DateTimeOffset.MaxValue)))
            .NewGroup(SettingsEntryCategory.Download)
            .Config(group => group
                .Int(t => t.MaximumDownloadHistoryRecords, 10, ushort.MaxValue, 1)
                .Bool(t => t.OverwriteDownloadedFile)
                .Int(t => t.MaxDownloadTaskConcurrencyLevel, 1, Environment.ProcessorCount, 1,
                    entry => entry.ValueChanged += t => App.AppViewModel.DownloadManager.ConcurrencyDegree = t)
                .DownloadMacro()
                .MultiValues(t => t.IllustrationDownloadFormat, entry =>
                    entry.Enum(
                            WorkTypeEnum.Illustration,
                            t => t.IllustrationDownloadFormat,
                            IllustrationDownloadFormat.Pairs)
                        .Enum(
                            WorkTypeEnum.Ugoira,
                            t => t.UgoiraDownloadFormat,
                            UgoiraDownloadFormat.Pairs)
                        .Enum(
                            WorkTypeEnum.Novel,
                            t => t.NovelDownloadFormat,
                            NovelDownloadFormat.Pairs))
                .Int(t => t.LossyImageDownloadQuality, -1, 100, 5)
                .Bool(t => t.DownloadWhenBookmarked))
            .NewGroup(SettingsEntryCategory.Misc)
            .Config(group => group
                .Int(t => t.MaximumBrowseHistoryRecords, 10, ushort.MaxValue, 10))
            .Build();
    }

    public IReadOnlyList<ISettingsGroup> LocalGroups { get; }

    public IReadOnlyList<ExtensionSettingsGroup> ExtensionGroups { get; } =
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>().SettingsGroups;

    public string UpdateInfo => AppInfo.AppVersion.UpdateState switch
    {
        UpdateState.MajorUpdate => SettingsPageResources.MajorUpdateAvailable,
        UpdateState.MinorUpdate => SettingsPageResources.MinorUpdateAvailable,
        UpdateState.BuildUpdate or UpdateState.RevisionUpdate => SettingsPageResources.BuildUpdateAvailable,
        UpdateState.Insider => SettingsPageResources.IsInsider,
        UpdateState.UpToDate => SettingsPageResources.IsUpToDate,
        _ => SettingsPageResources.UnknownUpdateState
    };

    public string? NewestVersion => AppInfo.AppVersion.UpdateAvailable
        ? AppInfo.AppVersion.NewestVersion?.Let(t => $"{t.Major}.{t.Minor}.{t.Build}.{t.Revision}")
        : null;

    public InfoBarSeverity UpdateInfoSeverity => AppInfo.AppVersion.UpdateState switch
    {
        UpdateState.MajorUpdate or UpdateState.MinorUpdate => InfoBarSeverity.Warning,
        UpdateState.UpToDate => InfoBarSeverity.Success,
        _ => InfoBarSeverity.Informational
    };

    public async void CheckForUpdate()
    {
        if (CheckingUpdate)
            return;
        var downloaded = false;
        try
        {
            _cancellationTokenSource = new CancellationTokenSource();
            CheckingUpdate = true;
            UpdateMessage = SettingsPageResources.CheckingForUpdate;
            using var client = new HttpClient();
            await AppInfo.AppVersion.GitHubCheckForUpdateAsync(client);
            var appReleaseModel = AppInfo.AppVersion.NewestAppReleaseModel;
            OnPropertyChanged(nameof(UpdateInfo));
            OnPropertyChanged(nameof(NewestVersion));
            OnPropertyChanged(nameof(UpdateInfoSeverity));
            OnPropertyChanged(nameof(LastCheckedUpdate));
            if (!AppInfo.AppVersion.UpdateAvailable)
            {
                UpdateMessage = null;
                return;
            }

            if (appReleaseModel?.ReleaseUri is null)
            {
                UpdateMessage = SettingsPageResources.UpdateFailed;
                return;
            }

            DownloadingUpdate = true;
            UpdateMessage = SettingsPageResources.DownloadingUpdate;
            var filePath = AppKnownFolders.Temp.CombinePath(appReleaseModel.ReleaseUri.Segments[^1]);
            await using var fileStream = FileHelper.CreateAsyncWrite(filePath);
            var exception = await client.DownloadStreamAsync(fileStream, appReleaseModel.ReleaseUri,
                new Progress<double>(progress => DownloadingUpdateProgress = progress), cancellationToken: _cancellationTokenSource.Token);
            // ReSharper disable once DisposeOnUsingVariable
            client.Dispose();

            DownloadingUpdate = false;
            DownloadingUpdateProgress = 0;

            if (exception is null)
            {
                downloaded = true;
                if (_cancellationTokenSource is { IsCancellationRequested: true })
                    return;
                if (await FrameworkElement.CreateOkCancelAsync(SettingsPageResources.UpdateApp,
                        SettingsPageResources.DownloadedAndWaitingToInstall.Format(appReleaseModel.Version)) is ContentDialogResult.Primary)
                {
                    var process = new Process
                    {
                        StartInfo =
                        {
                            FileName = filePath,
                            Verb = "runas",
                            UseShellExecute = true
                        }
                    };
                    _ = process.Start();
                    await process.WaitForExitAsync();
                    UpdateMessage = null;
                }
                else
                    UpdateMessage = SettingsPageResources.InstallCanceled;
            }
        }
        catch
        {
            if (downloaded)
            {
                UpdateMessage = SettingsPageResources.UpdateFailedInTempFolder;
                ExpandExpander = true;
            }
            else
                UpdateMessage = SettingsPageResources.UpdateFailed;
        }
        finally
        {
            CheckingUpdate = false;
            CancelToken();
        }
    }

    public string GetLastUpdateCheckDisplayString(DateTimeOffset lastChecked)
    {
        return SettingsPageResources.LastCheckedPrefix + lastChecked.ToString(AppSettings.CurrentCulture);
    }

    public void ShowClearData(ClearDataKind kind)
    {
        FrameworkElement.SuccessGrowl(SettingsPageResources.GetResource(kind));
    }

    public void CancelToken()
    {
        var cts = _cancellationTokenSource;
        _cancellationTokenSource = null;
        cts?.TryCancelDispose();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        CancelToken();
    }
}

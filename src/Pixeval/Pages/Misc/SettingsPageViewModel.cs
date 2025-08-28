// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.Extensions;
using Pixeval.Options;
using Pixeval.Settings;
using Pixeval.Settings.Models;
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
        LocalGroups =
        [
            new(SettingsEntryCategory.Application)
            {
                new EnumAppSettingsEntry(AppSettings,
                    t => t.Theme,
                    ElementThemeExtension.GetItems()) { ValueChanged = t => WindowFactory.SetTheme((ElementTheme)t) },
                new EnumAppSettingsEntry(AppSettings,
                    t => t.Backdrop,
                    BackdropTypeExtension.GetItems())
                {
                    ValueChanged = t => WindowFactory.SetBackdrop((BackdropType)t)
                },
                new FontAppSettingsEntry(AppSettings,
                    t => t.AppFontFamilyName),
                new LanguageAppSettingsEntry(),
                new IpWithSwitchAppSettingsEntry(AppSettings)
                {
                    ValueChanged = t => App.AppViewModel.MakoClient.Configuration.DomainFronting = t
                },
                new ProxyAppSettingsEntry(AppSettings)
                {
                    ProxyChanged = t => App.AppViewModel.MakoClient.Configuration.Proxy = t
                },
                new StringAppSettingsEntry(AppSettings,
                    t => t.MirrorHost)
                {
                    Placeholder = SettingsPageResources.ImageMirrorServerTextBoxPlaceholderText,
                    ValueChanged = t => App.AppViewModel.MakoClient.Configuration.MirrorHost = t
                },
                new BoolAppSettingsEntry(AppSettings,
                    t => t.UseFileCache),
                new EnumAppSettingsEntry(AppSettings,
                    t => t.DefaultSelectedTabItem,
                    MainPageTabItemExtension.GetItems()),
                new StringAppSettingsEntry(AppSettings,
                    t => t.WebCookie)
                {
                    Placeholder = SettingsPageResources.WebCookieTextBoxPlaceholderText,
                    ValueChanged = t => App.AppViewModel.MakoClient.Configuration.Cookie = t
                },
                new IntAppSettingsEntry(AppSettings,
                    t => t.NavigationViewOpenPaneWidth)
                {
                    Max = 1000,
                    // 48时图标会被挡住一部分
                    Min = 50,
                    SmallChange = 10,
                    ValueChanged = t => MainPage.Current.NavigationView.OpenPaneLength = t
                },
                new BoolAppSettingsEntry(AppSettings,
                    t => t.ReconfirmationOfClosingWindow)
            },
            new(SettingsEntryCategory.BrowsingExperience)
            {
                new EnumAppSettingsEntry(AppSettings,
                    t => t.ThumbnailDirection,
                    ThumbnailDirectionExtension.GetItems()),
                new EnumAppSettingsEntry(AppSettings,
                    t => t.ItemsViewLayoutType,
                    ItemsViewLayoutTypeExtension.GetItems()),
                new EnumAppSettingsEntry(AppSettings,
                    t => t.TargetFilter,
                    TargetFilterExtension.GetItems()),
                new MultiStringsAppSettingsEntry(AppSettings,
                    t => t.BlockedTags,
                    v => [..v.BlockedTags],
                    (v, o) => v.BlockedTags = [.. o])
                {
                    Placeholder = SettingsPageResources.BlockedTagsTokenizingTextBoxPlaceholderText
                },
                new BoolAppSettingsEntry(AppSettings,
                    t => t.BrowseOriginalImage),
                new DoubleAppSettingsEntry(AppSettings,
                    t => t.ScrollRate)
                {
                    Max = 10,
                    Min = 0,
                    ValueChanged = d => AdvancedItemsView.ScrollRate = (float)d
                },
                new BoolAppSettingsEntry(AppSettings,
                    t => t.OpenWorkInfoByDefault),
            },
            new(SettingsEntryCategory.Search)
            {
                new StringAppSettingsEntry(AppSettings,
                    t => t.ReverseSearchApiKey)
                {
                    DescriptionUri = new("https://saucenao.com/user.php?page=search-api"),
                    Placeholder = SettingsPageResources.ReverseSearchApiKeyTextBoxPlaceholderText
                },
                new IntAppSettingsEntry(AppSettings,
                    t => t.ReverseSearchResultSimilarityThreshold)
                {
                    Max = 100,
                    Min = 1
                },
                new IntAppSettingsEntry(AppSettings,
                    t => t.MaximumSearchHistoryRecords)
                {
                    Max = 200,
                    Min = 10
                },
                new IntAppSettingsEntry(AppSettings,
                    t => t.MaximumSuggestionBoxSearchHistory)
                {
                    Max = 20,
                    Min = 0
                },
                new EnumAppSettingsEntry(AppSettings,
                    t => t.WorkSortOption,
                    WorkSortOptionExtension.GetItems()),
                new EnumAppSettingsEntry(AppSettings,
                    t => t.SimpleWorkType,
                    SimpleWorkTypeExtension.GetItems()),
                new MultiValuesEntry(t => t.IllustrationRankOption,
                    [
                        new EnumAppSettingsEntry(AppSettings,
                            WorkTypeEnum.Illustration,
                            t => t.IllustrationRankOption,
                            IllustrationRankOptionExtension.GetItems()),
                        new EnumAppSettingsEntry(AppSettings,
                            WorkTypeEnum.Novel,
                            t => t.NovelRankOption,
                            NovelRankOptionExtension.GetItems())
                    ]),
                new MultiValuesEntry(t => t.SearchIllustrationTagMatchOption,
                    [
                        new EnumAppSettingsEntry(AppSettings,
                            WorkTypeEnum.Illustration,
                            t => t.SearchIllustrationTagMatchOption,
                            SearchIllustrationTagMatchOptionExtension.GetItems()),
                        new EnumAppSettingsEntry(AppSettings,
                            WorkTypeEnum.Novel,
                            t => t.SearchNovelTagMatchOption,
                            SearchNovelTagMatchOptionExtension.GetItems())
                    ]),
                new DateWithSwitchAppSettingsEntry(AppSettings,
                    t => t.UseSearchStartDate,
                    t => t.SearchStartDate),
                new DateWithSwitchAppSettingsEntry(AppSettings,
                    t => t.UseSearchEndDate,
                    t => t.SearchEndDate)
            },
            new(SettingsEntryCategory.Download)
            {
                new IntAppSettingsEntry(AppSettings,
                    t => t.MaximumDownloadHistoryRecords)
                {
                    Max = ushort.MaxValue,
                    Min = 10
                },
                new BoolAppSettingsEntry(AppSettings,
                    t => t.OverwriteDownloadedFile),
                new IntAppSettingsEntry(AppSettings,
                    t => t.MaxDownloadTaskConcurrencyLevel)
                {
                    Max = Environment.ProcessorCount,
                    Min = 1,
                    ValueChanged = t => App.AppViewModel.DownloadManager.ConcurrencyDegree = t
                },
                new DownloadMacroAppSettingsEntry(AppSettings),
                new MultiValuesEntry(t => t.IllustrationDownloadFormat,
                    [
                        new EnumAppSettingsEntry(AppSettings,
                            WorkTypeEnum.Illustration,
                            t => t.IllustrationDownloadFormat,
                            IllustrationDownloadFormatExtension.GetItems()),
                        new EnumAppSettingsEntry(AppSettings,
                            WorkTypeEnum.Ugoira,
                            t => t.UgoiraDownloadFormat,
                            UgoiraDownloadFormatExtension.GetItems()),
                        new EnumAppSettingsEntry(AppSettings,
                            WorkTypeEnum.Novel,
                            t => t.NovelDownloadFormat,
                            NovelDownloadFormatExtension.GetItems())
                    ]),
                new IntAppSettingsEntry(AppSettings,
                    t => t.LossyImageDownloadQuality)
                {
                    Max = 100,
                    Min = -1
                },
                new BoolAppSettingsEntry(AppSettings,
                    t => t.DownloadWhenBookmarked)
            },
            new(SettingsEntryCategory.Misc)
            {
                new IntAppSettingsEntry(AppSettings,
                    t => t.MaximumBrowseHistoryRecords)
                {
                    Placeholder = SettingsPageResources.MaximumBrowseHistoryRecordsNumerBoxPlaceholderText,
                    Max = ushort.MaxValue,
                    Min = 10
                }
            }
        ];
    }

    public SimpleSettingsGroup[] LocalGroups { get; }

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
        FrameworkElement.SuccessGrowl(ClearDataKindExtension.GetResource(kind));
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

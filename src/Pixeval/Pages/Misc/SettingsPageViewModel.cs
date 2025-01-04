#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/SettingsPageViewModel.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Options;
using Pixeval.Util.ComponentModels;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.Settings;
using Windows.System;
using Microsoft.UI.Xaml;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Settings.Models;
using WinUI3Utilities;
using Symbol = FluentIcons.Common.Symbol;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Extensions;

namespace Pixeval.Pages.Misc;

public partial class SettingsPageViewModel : UiObservableObject, IDisposable
{
    public DateTimeOffset LastCheckedUpdate
    {
        get => AppSettings.LastCheckedUpdate;
        set => SetProperty(AppSettings.LastCheckedUpdate, value, AppSettings, (setting, v) =>
        {
            setting.LastCheckedUpdate = v;
            AppInfo.LocalConfig[nameof(AppSettings.LastCheckedUpdate)] = v;
        });
    }

    public bool DownloadUpdateAutomatically
    {
        get => AppSettings.DownloadUpdateAutomatically;
        set => SetProperty(AppSettings.DownloadUpdateAutomatically, value, AppSettings, (setting, v) =>
        {
            setting.DownloadUpdateAutomatically = v;
            AppInfo.LocalConfig[nameof(AppSettings.DownloadUpdateAutomatically)] = v;
        });
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
    public SettingsPageViewModel(ulong hWnd) : base(hWnd)
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
                    t => t.WebCookie) { Placeholder = SettingsPageResources.WebCookieTextBoxPlaceholderText }
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
                new ClickableAppSettingsEntry(
                    SettingsPageResources.ViewingRestrictionEntryHeader,
                    SettingsPageResources.ViewingRestrictionEntryDescription,
                    Symbol.SubtractCircle,
                    () => _ = Launcher.LaunchUriAsync(new("https://www.pixiv.net/settings/viewing")))
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
                new MultiValuesEntry(SettingsPageResources.RankOptionEntryHeader,
                    SettingsPageResources.RankOptionEntryDescription,
                    Symbol.ArrowTrending,
                    [
                        new EnumAppSettingsEntry(AppSettings,
                            WorkTypeEnum.Illustration,
                            t => t.IllustrationRankOption,
                            RankOptionExtension.GetItems()),
                        new EnumAppSettingsEntry(AppSettings,
                            WorkTypeEnum.Novel,
                            t => t.NovelRankOption,
                            NovelRankOptionExtension.GetItems())
                    ]),
                new MultiValuesEntry(SettingsPageResources.DefaultSearchTagMatchOptionEntryHeader,
                    SettingsPageResources.DefaultSearchTagMatchOptionEntryDescription,
                    Symbol.CheckmarkCircleSquare,
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
                new MultiValuesEntry(SettingsPageResources.WorkDownloadFormatEntryHeader,
                    SettingsPageResources.WorkDownloadFormatEntryDescription,
                    Symbol.TextPeriodAsterisk,
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
            await using var fileStream = IoHelper.OpenAsyncWrite(filePath);
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
                if (await HWnd.CreateOkCancelAsync(SettingsPageResources.UpdateApp,
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
        HWnd.SuccessGrowl(ClearDataKindExtension.GetResource(kind));
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

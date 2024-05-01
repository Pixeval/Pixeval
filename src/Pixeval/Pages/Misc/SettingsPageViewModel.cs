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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.ComponentModels;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;
using Pixeval.Settings;
using System.Collections.Generic;
using Windows.System;
using Microsoft.UI.Xaml;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi.Global.Enum;
using WinUI3Utilities;
using WinUI3Utilities.Controls;

namespace Pixeval.Pages.Misc;

public partial class SettingsPageViewModel : UiObservableObject, IDisposable
{
    public DateTimeOffset LastCheckedUpdate
    {
        get => AppSetting.LastCheckedUpdate;
        set => SetProperty(AppSetting.LastCheckedUpdate, value, AppSetting, (@setting, @value) => @setting.LastCheckedUpdate = @value);
    }
    
    public bool DownloadUpdateAutomatically
    {
        get => AppSetting.DownloadUpdateAutomatically;
        set => SetProperty(AppSetting.DownloadUpdateAutomatically, value, AppSetting, (@setting, @value) => @setting.DownloadUpdateAutomatically = @value);
    }

    public AppSettings AppSetting => App.AppViewModel.AppSettings;

    [ObservableProperty] private bool _checkingUpdate;

    [ObservableProperty] private bool _downloadingUpdate;

    [ObservableProperty] private double _downloadingUpdateProgress;

    [ObservableProperty] private string? _updateMessage;

    [ObservableProperty] private bool _expandExpander;

    private CancellationHandle? _cancellationHandle;

    /// <inheritdoc/>
    public SettingsPageViewModel(ulong hWnd) : base(hWnd)
    {
        Entries =
        [
            new(SettingsPageResources.ApplicationSettingsGroupText)
            {
                new EnumAppSettingsEntry<ElementTheme>(AppSetting,
                    SettingsPageResources.ThemeEntryHeader,
                    SettingsPageResources.ThemeEntryDescriptionHyperlinkButtonContent,
                    IconGlyph.PersonalizeE771,
                    nameof(AppSettings.Theme)) { ValueChanged = t => WindowFactory.SetTheme((ElementTheme)t) },
                new EnumAppSettingsEntry<BackdropType>(AppSetting,
                    SettingsPageResources.BackdropEntryHeader,
                    "",
                    IconGlyph.ColorE790,
                    nameof(AppSettings.Backdrop)) { ValueChanged = t => WindowFactory.SetBackdrop((BackdropType)t) },
                new FontAppSettingsEntry(AppSetting),
                new LanguageAppSettingsEntry(AppSetting),
                new IpWithSwitchAppSettingsEntry(AppSetting)
                {
                    ValueChanged = t => App.AppViewModel.MakoClient.Configuration.DomainFronting = t
                },
                new BoolAppSettingsEntry(AppSetting,
                    SettingsPageResources.UseFileCacheEntryHeader,
                    SettingsPageResources.UseFileCacheEntryDescription,
                    IconGlyph.FileExplorerEC50,
                    nameof(AppSettings.UseFileCache)),
                new BoolAppSettingsEntry(AppSetting,
                    SettingsPageResources.GenerateHelpLinkEntryHeader,
                    SettingsPageResources.GenerateHelpLinkEntryDescription,
                    IconGlyph.LinkE71B,
                    nameof(AppSettings.DisplayTeachingTipWhenGeneratingAppLink)),
                new EnumAppSettingsEntry<MainPageTabItem>(AppSetting,
                    SettingsPageResources.DefaultSelectedTabEntryHeader,
                    SettingsPageResources.DefaultSelectedTabEntryDescription,
                    IconGlyph.CheckMarkE73E,
                    nameof(AppSettings.DefaultSelectedTabItem))
            },
            new(SettingsPageResources.BrowsingExperienceSettingsGroupText)
            {
                new EnumAppSettingsEntry<ThumbnailDirection>(AppSetting,
                    SettingsPageResources.ThumbnailDirectionEntryHeader,
                    SettingsPageResources.ThumbnailDirectionEntryDescription,
                    IconGlyph.TypeE97C,
                    nameof(AppSettings.ThumbnailDirection)),
                new EnumAppSettingsEntry<ItemsViewLayoutType>(AppSetting,
                    SettingsPageResources.ItemsViewLayoutTypeEntryHeader,
                    SettingsPageResources.ItemsViewLayoutTypeEntryDescription,
                    IconGlyph.TilesECA5,
                    nameof(AppSettings.ItemsViewLayoutType)),
                new EnumAppSettingsEntry<TargetFilter>(AppSetting,
                    SettingsPageResources.TargetAPIPlatformEntryHeader,
                    SettingsPageResources.TargetAPIPlatformEntryDescription,
                    IconGlyph.CommandPromptE756,
                    nameof(AppSettings.TargetFilter)),
                new TokenizingAppSettingsEntry(AppSetting),
                new ClickableAppSettingsEntry(AppSetting,
                    SettingsPageResources.ViewingRestrictionEntryHeader,
                    SettingsPageResources.ViewingRestrictionEntryDescription,
                    IconGlyph.BlockContactE8F8,
                    () => Launcher.LaunchUriAsync(new Uri("https://www.pixiv.net/setting_user.php")))
            },
            new(SettingsPageResources.SearchSettingsGroupText)
            {
                new IntAppSettingsEntry(AppSetting,
                    SettingsPageResources.ReverseSearchResultSimilarityThresholdEntryHeader,
                    SettingsPageResources.ReverseSearchResultSimilarityThresholdEntryDescription,
                    IconGlyph.FilterE71C,
                    nameof(AppSettings.ReverseSearchResultSimilarityThreshold))
                {
                    Max = 100,
                    Min = 1
                },
                new StringAppSettingsEntry(AppSetting,
                    SettingsPageResources.ReverseSearchApiKeyEntryHeader,
                    SettingsPageResources.ReverseSearchApiKeyEntryDescriptionHyperlinkButtonContent,
                    IconGlyph.SearchAndAppsE773,
                    nameof(AppSettings.ReverseSearchApiKey))
                {
                    DescriptionUri = new Uri("https://saucenao.com/user.php?page=search-api"),
                    Placeholder = SettingsPageResources.ReverseSearchApiKeyTextBoxPlaceholderText
                },
                new IntAppSettingsEntry(AppSetting,
                    SettingsPageResources.MaximumSearchHistoryRecordsEntryHeader,
                    SettingsPageResources.MaximumSearchHistoryRecordsEntryDescription,
                    IconGlyph.HistoryE81C,
                    nameof(AppSettings.MaximumSearchHistoryRecords))
                {
                    Max = 200,
                    Min = 10
                },
                new IntAppSettingsEntry(AppSetting,
                    SettingsPageResources.MaximumSuggestionBoxSearchHistoryEntryHeader,
                    SettingsPageResources.MaximumSuggestionBoxSearchHistoryEntryDescription,
                    IconGlyph.SetHistoryStatus2F739,
                    nameof(AppSettings.MaximumSuggestionBoxSearchHistory))
                {
                    Max = 20,
                    Min = 0
                },
                new EnumAppSettingsEntry<WorkSortOption>(AppSetting,
                    SettingsPageResources.DefaultSearchSortOptionEntryHeader,
                    SettingsPageResources.DefaultSearchSortOptionEntryDescription,
                    IconGlyph.SortE8CB,
                    nameof(AppSettings.WorkSortOption)),
                new EnumAppSettingsEntry<SimpleWorkType>(AppSetting,
                    SettingsPageResources.SimpleWorkTypeEntryHeader,
                    SettingsPageResources.SimpleWorkTypeEntryDescription,
                    IconGlyph.ViewAllE8A9,
                    nameof(AppSettings.SimpleWorkType)),
                new MultiValuesAppSettingsEntry(AppSetting,
                    SettingsPageResources.RankOptionEntryHeader,
                    SettingsPageResources.RankOptionEntryDescription,
                    IconGlyph.MarketEAFC,
                    [
                        new EnumAppSettingsEntry<IllustrationDownloadFormat>(AppSetting,
                            WorkTypeEnum.Illustration,
                            nameof(AppSettings.IllustrationDownloadFormat)),
                        new EnumAppSettingsEntry<NovelDownloadFormat>(AppSetting,
                            WorkTypeEnum.Novel,
                            nameof(AppSettings.NovelDownloadFormat))
                    ]),
                new MultiValuesAppSettingsEntry(AppSetting,
                    SettingsPageResources.DefaultSearchTagMatchOptionEntryHeader,
                    SettingsPageResources.DefaultSearchTagMatchOptionEntryDescription,
                    IconGlyph.PassiveAuthenticationF32A,
                    [
                        new EnumAppSettingsEntry<SearchIllustrationTagMatchOption>(AppSetting,
                            WorkTypeEnum.Illustration,
                            nameof(AppSettings.SearchIllustrationTagMatchOption)),
                        new EnumAppSettingsEntry<SearchNovelTagMatchOption>(AppSetting,
                            WorkTypeEnum.Novel,
                            nameof(AppSettings.SearchNovelTagMatchOption))
                    ]),
                new EnumAppSettingsEntry<SearchDuration>(AppSetting,
                    SettingsPageResources.SearchDurationEntryHeader,
                    SettingsPageResources.SearchDurationEntryDescription,
                    IconGlyph.EaseOfAccessE776,
                    nameof(AppSettings.SearchDuration)),
                new DateRangeWithSwitchAppSettingsEntry(AppSetting)
            },
            new(SettingsPageResources.DownloadSettingsGroupText)
            {
                new IntAppSettingsEntry(AppSetting,
                    SettingsPageResources.MaximumDownloadHistoryRecordsEntryHeader,
                    SettingsPageResources.MaximumDownloadHistoryRecordsEntryDescription,
                    IconGlyph.HistoryE81C,
                    nameof(AppSettings.MaximumDownloadHistoryRecords))
                {
                    Max = 200,
                    Min = 10
                },
                new BoolAppSettingsEntry(AppSetting,
                    SettingsPageResources.OverwriteDownloadedFileEntryHeader,
                    SettingsPageResources.OverwriteDownloadedFileEntryDescription,
                    IconGlyph.ScanE8FE,
                    nameof(AppSettings.OverwriteDownloadedFile)),
                new IntAppSettingsEntry(AppSetting,
                    SettingsPageResources.MaxDownloadConcurrencyLevelEntryHeader,
                    SettingsPageResources.MaxDownloadConcurrencyLevelEntryDescription,
                    IconGlyph.LightningBoltE945,
                    nameof(AppSettings.MaxDownloadTaskConcurrencyLevel))
                {
                    Max = Environment.ProcessorCount,
                    Min = 1,
                    ValueChanged = t => App.AppViewModel.DownloadManager.ConcurrencyDegree = t
                },
                new DownloadMacroAppSettingsEntry(AppSetting),
                new MultiValuesAppSettingsEntry(AppSetting,
                    SettingsPageResources.WorkDownloadFormatEntryHeader,
                    SettingsPageResources.WorkDownloadFormatEntryDescription,
                    IconGlyph.CaptionE8BA,
                    [
                        new EnumAppSettingsEntry<IllustrationDownloadFormat>(AppSetting,
                            WorkTypeEnum.Illustration,
                            nameof(AppSettings.IllustrationDownloadFormat)),
                        new EnumAppSettingsEntry<UgoiraDownloadFormat>(AppSetting,
                            WorkTypeEnum.Ugoira,
                            nameof(AppSettings.UgoiraDownloadFormat)),
                        new EnumAppSettingsEntry<NovelDownloadFormat>(AppSetting,
                            WorkTypeEnum.Novel,
                            nameof(AppSettings.NovelDownloadFormat))
                    ]),
                new BoolAppSettingsEntry(AppSetting,
                    SettingsPageResources.DownloadWhenBookmarkedEntryHeader,
                    SettingsPageResources.DownloadWhenBookmarkedEntryDescription,
                    IconGlyph.SaveLocalE78C,
                    nameof(AppSettings.DownloadWhenBookmarked))
            },
            new(SettingsPageResources.MiscSettingsGroupText)
            {
                new IntAppSettingsEntry(AppSetting,
                    SettingsPageResources.MaximumBrowseHistoryRecordsEntryHeader,
                    SettingsPageResources.MaximumBrowseHistoryRecordsEntryDescription,
                    IconGlyph.HistoryE81C,
                    nameof(AppSettings.MaximumBrowseHistoryRecords))
                {
                    Max = 200,
                    Min = 10
                },
                new StringAppSettingsEntry(AppSetting,
                    SettingsPageResources.ImageMirrorServerEntryHeader,
                    SettingsPageResources.ImageMirrorServerEntryDescription,
                    IconGlyph.HardDriveEDA2,
                    nameof(AppSettings.MirrorHost))
                {
                    Placeholder = SettingsPageResources.ImageMirrorServerTextBoxPlaceholderText,
                    ValueChanged = t => App.AppViewModel.MakoClient.Configuration.MirrorHost = t
                }
            }
        ];
    }

    public IEnumerable<SimpleSettingsGroup> Entries { get; }

    public string UpdateInfo => AppInfo.AppVersion.UpdateState switch
    {
        UpdateState.MajorUpdate => SettingsPageResources.MajorUpdateAvailable,
        UpdateState.MinorUpdate => SettingsPageResources.MinorUpdateAvailable,
        UpdateState.BuildUpdate or UpdateState.SpecifierUpdate => SettingsPageResources.BuildUpdateAvailable,
        UpdateState.Insider => SettingsPageResources.IsInsider,
        UpdateState.UpToDate => SettingsPageResources.IsUpToDate,
        _ => SettingsPageResources.UnknownUpdateState
    };

    public string? NewestVersion => AppInfo.AppVersion.UpdateAvailable ? AppInfo.AppVersion.NewestVersion?.ToString() : null;

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
            _cancellationHandle = new CancellationHandle();
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
            var result = await client.DownloadStreamAsync(appReleaseModel.ReleaseUri.OriginalString,
                new Progress<double>(progress => DownloadingUpdateProgress = progress), _cancellationHandle);
            // ReSharper disable once DisposeOnUsingVariable
            client.Dispose();

            DownloadingUpdate = false;
            DownloadingUpdateProgress = 0;

            if (result is Result<Stream>.Success { Value: var value })
            {
                var filePath = Path.Combine(AppKnownFolders.Temporary.Self.Path, appReleaseModel.ReleaseUri.Segments[^1]);
                await using (var fileStream = File.OpenWrite(filePath))
                    await value.CopyToAsync(fileStream);

                downloaded = true;
                if (_cancellationHandle is { IsCancelled: true })
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
        return SettingsPageResources.LastCheckedPrefix + lastChecked.ToString(CultureInfo.CurrentUICulture);
    }

    public void ShowClearData(ClearDataKind kind)
    {
        HWnd.SuccessGrowl(kind.GetLocalizedResourceContent()!);
    }

    public void CancelToken()
    {
        _cancellationHandle?.Cancel();
        _cancellationHandle = null;
    }

    public void Dispose()
    {
        CancelToken();
    }
}

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
using Windows.System;
using Microsoft.UI.Xaml;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Settings.Models;
using WinUI3Utilities;
using WinUI3Utilities.Controls;

namespace Pixeval.Pages.Misc;

public partial class SettingsPageViewModel : UiObservableObject, IDisposable
{
    public DateTimeOffset LastCheckedUpdate
    {
        get => AppSettings.LastCheckedUpdate;
        set => SetProperty(AppSettings.LastCheckedUpdate, value, AppSettings, (@setting, @value) => @setting.LastCheckedUpdate = @value);
    }

    public bool DownloadUpdateAutomatically
    {
        get => AppSettings.DownloadUpdateAutomatically;
        set => SetProperty(AppSettings.DownloadUpdateAutomatically, value, AppSettings, (@setting, @value) => @setting.DownloadUpdateAutomatically = @value);
    }

    public AppSettings AppSettings => App.AppViewModel.AppSettings;

    [ObservableProperty] private bool _checkingUpdate;

    [ObservableProperty] private bool _downloadingUpdate;

    [ObservableProperty] private double _downloadingUpdateProgress;

    [ObservableProperty] private string? _updateMessage;

    [ObservableProperty] private bool _expandExpander;

    private CancellationHandle? _cancellationHandle;

    /// <inheritdoc/>
    public SettingsPageViewModel(ulong hWnd) : base(hWnd)
    {
        Groups =
        [
            new(SettingsEntryCategory.Application)
            {
                new EnumAppSettingsEntry<ElementTheme>(AppSettings,
                    t => t.Theme) { ValueChanged = t => WindowFactory.SetTheme((ElementTheme)t) },
                new EnumAppSettingsEntry<BackdropType>(AppSettings,
                    t => t.Backdrop) { ValueChanged = t => WindowFactory.SetBackdrop((BackdropType)t) },
                new FontAppSettingsEntry(AppSettings,
                    t => t.AppFontFamilyName),
                new LanguageAppSettingsEntry(AppSettings),
                new IpWithSwitchAppSettingsEntry(AppSettings)
                {
                    ValueChanged = t => App.AppViewModel.MakoClient.Configuration.DomainFronting = t
                },
                new BoolAppSettingsEntry(AppSettings,
                    t => t.UseFileCache),
                new EnumAppSettingsEntry<MainPageTabItem>(AppSettings,
                    t => t.DefaultSelectedTabItem)
            },
            new(SettingsEntryCategory.BrowsingExperience)
            {
                new EnumAppSettingsEntry<ThumbnailDirection>(AppSettings,
                    t => t.ThumbnailDirection),
                new EnumAppSettingsEntry<ItemsViewLayoutType>(AppSettings,
                    t => t.ItemsViewLayoutType),
                new EnumAppSettingsEntry<TargetFilter>(AppSettings,
                    t => t.TargetFilter),
                new TokenizingAppSettingsEntry(AppSettings),
                new BoolAppSettingsEntry(AppSettings,
                    t => t.BrowserOriginalImage),
                new ClickableAppSettingsEntry(AppSettings,
                    SettingsPageResources.ViewingRestrictionEntryHeader,
                    SettingsPageResources.ViewingRestrictionEntryDescription,
                    IconGlyph.BlockContactE8F8,
                    () => _ = Launcher.LaunchUriAsync(new Uri("https://www.pixiv.net/setting_user.php")))
            },
            new(SettingsEntryCategory.Search)
            {
                new StringAppSettingsEntry(AppSettings,
                    t => t.ReverseSearchApiKey)
                {
                    DescriptionUri = new Uri("https://saucenao.com/user.php?page=search-api"),
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
                new EnumAppSettingsEntry<WorkSortOption>(AppSettings,
                    t => t.WorkSortOption),
                new EnumAppSettingsEntry<SimpleWorkType>(AppSettings,
                    t => t.SimpleWorkType),
                new MultiValuesAppSettingsEntry(AppSettings,
                    SettingsPageResources.RankOptionEntryHeader,
                    SettingsPageResources.RankOptionEntryDescription,
                    IconGlyph.MarketEAFC,
                    [
                        new EnumAppSettingsEntry<RankOption>(AppSettings,
                            WorkTypeEnum.Illustration,
                            t => t.IllustrationRankOption),
                        new EnumAppSettingsEntry(AppSettings,
                            WorkTypeEnum.Novel,
                            t => t.NovelRankOption,
                            RankOptionExtension.NovelRankOptions)
                    ]),
                new MultiValuesAppSettingsEntry(AppSettings,
                    SettingsPageResources.DefaultSearchTagMatchOptionEntryHeader,
                    SettingsPageResources.DefaultSearchTagMatchOptionEntryDescription,
                    IconGlyph.PassiveAuthenticationF32A,
                    [
                        new EnumAppSettingsEntry<SearchIllustrationTagMatchOption>(AppSettings,
                            WorkTypeEnum.Illustration,
                            t => t.SearchIllustrationTagMatchOption),
                        new EnumAppSettingsEntry<SearchNovelTagMatchOption>(AppSettings,
                            WorkTypeEnum.Novel,
                            t => t.SearchNovelTagMatchOption)
                    ]),
                new EnumAppSettingsEntry<SearchDuration>(AppSettings,
                    t => t.SearchDuration),
                new DateRangeWithSwitchAppSettingsEntry(AppSettings)
            },
            new(SettingsEntryCategory.Download)
            {
                new IntAppSettingsEntry(AppSettings,
                    t => t.MaximumDownloadHistoryRecords)
                {
                    Max = 200,
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
                new MultiValuesAppSettingsEntry(AppSettings,
                    SettingsPageResources.WorkDownloadFormatEntryHeader,
                    SettingsPageResources.WorkDownloadFormatEntryDescription,
                    IconGlyph.CaptionE8BA,
                    [
                        new EnumAppSettingsEntry<IllustrationDownloadFormat>(AppSettings,
                            WorkTypeEnum.Illustration,
                            t => t.IllustrationDownloadFormat),
                        new EnumAppSettingsEntry<UgoiraDownloadFormat>(AppSettings,
                            WorkTypeEnum.Ugoira,
                            t => t.UgoiraDownloadFormat),
                        new EnumAppSettingsEntry<NovelDownloadFormat>(AppSettings,
                            WorkTypeEnum.Novel,
                            t => t.NovelDownloadFormat)
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
                    Max = 200,
                    Min = 10
                },
                new StringAppSettingsEntry(AppSettings,
                    t => t.MirrorHost)
                {
                    Placeholder = SettingsPageResources.ImageMirrorServerTextBoxPlaceholderText,
                    ValueChanged = t => App.AppViewModel.MakoClient.Configuration.MirrorHost = t
                }
            }
        ];
    }

    public SimpleSettingsGroup[] Groups { get; }

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

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
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using Windows.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Download.MacroParser;
using Pixeval.Misc;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.ComponentModels;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Pixeval.Download.Macros;
using Pixeval.Utilities.Threading;

namespace Pixeval.Pages.Misc;

[SettingsViewModel<AppSettings>(nameof(AppSetting))]
public partial class SettingsPageViewModel(FrameworkElement frameworkElement) : UiObservableObject(frameworkElement), IDisposable
{
    private static readonly IDictionary<string, string> _macroTooltips = new Dictionary<string, string>
    {
        ["ext"] = SettingsPageResources.ExtMacroTooltip,
        ["id"] = SettingsPageResources.IdMacroTooltip,
        ["title"] = SettingsPageResources.TitleMacroTooltip,
        ["artist_id"] = SettingsPageResources.ArtistIdMacroTooltip,
        ["artist_name"] = SettingsPageResources.ArtistNameMacroTooltip,
        ["if_r18"] = SettingsPageResources.IfR18MacroTooltip,
        ["if_r18g"] = SettingsPageResources.IfR18GMacroTooltip,
        ["if_ai"] = SettingsPageResources.IfAiMacroTooltip,
        ["if_illust"] = SettingsPageResources.IfIllustMacroTooltip,
        ["if_novel"] = SettingsPageResources.IfNovelMacroTooltip,
        ["if_manga"] = SettingsPageResources.IfMangaMacroTooltip,
        ["if_gif"] = SettingsPageResources.IfGifMacroTooltip,
        ["manga_index"] = SettingsPageResources.MangaIndexMacroTooltip
    };

    public static IEnumerable<string> AvailableFonts { get; }

    public static ICollection<StringRepresentableItem> AvailableMacros { get; }

    public static IEnumerable<CultureInfo> AvailableCultures { get; }

    public static IEnumerable<LanguageModel> AvailableLanguages { get; } = [LanguageModel.DefaultLanguage, new("简体中文", "zh-Hans-CN"), new("Русский", "ru"), new("English (United States)", "en-US")];

    public ObservableCollection<string> PixivAppApiNameResolver { get; set; } = [.. App.AppViewModel.AppSettings.PixivAppApiNameResolver];

    public ObservableCollection<string> PixivImageNameResolver { get; set; } = [.. App.AppViewModel.AppSettings.PixivImageNameResolver];

    public ObservableCollection<string> PixivImageNameResolver2 { get; set; } = [.. App.AppViewModel.AppSettings.PixivImageNameResolver2];

    public ObservableCollection<string> PixivOAuthNameResolver { get; set; } = [.. App.AppViewModel.AppSettings.PixivOAuthNameResolver];

    public ObservableCollection<string> PixivAccountNameResolver { get; set; } = [.. App.AppViewModel.AppSettings.PixivAccountNameResolver];

    public ObservableCollection<string> PixivWebApiNameResolver { get; set; } = [.. App.AppViewModel.AppSettings.PixivWebApiNameResolver];

    public AppSettings AppSetting { get; set; } = App.AppViewModel.AppSettings with { };

    [ObservableProperty] private bool _checkingUpdate;

    [ObservableProperty] private bool _downloadingUpdate;

    [ObservableProperty] private double _downloadingUpdateProgress;

    [ObservableProperty] private string? _updateMessage;

    [ObservableProperty] private bool _expandExpander;

    private CancellationHandle? _cancellationHandle;

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

    static SettingsPageViewModel()
    {
        AvailableCultures = [CultureInfo.GetCultureInfo("zh-cn")];
        using var collection = new InstalledFontCollection();
        AvailableFonts = collection.Families.Select(t => t.Name);
        AvailableMacros = MetaPathMacroAttributeHelper.GetAttachedTypeInstances()
            .Select(m => new StringRepresentableItem(_macroTooltips[m.Name], $"@{{{(m is IPredicate ? $"{m.Name}=" : m.Name)}}}"))
            .ToList();
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(DisableDomainFronting):
                App.AppViewModel.MakoClient.Configuration.Bypass = !DisableDomainFronting;
                break;
            case nameof(MirrorHost):
                App.AppViewModel.MakoClient.Configuration.MirrorHost = MirrorHost;
                break;
            case nameof(MaxDownloadTaskConcurrencyLevel):
                App.AppViewModel.DownloadManager.ConcurrencyDegree = MaxDownloadTaskConcurrencyLevel;
                break;
        }
    }

    public LanguageModel AppLanguage
    {
        get => AvailableLanguages.FirstOrDefault(t => t.Name == ApplicationLanguages.PrimaryLanguageOverride) ?? LanguageModel.DefaultLanguage;
        set
        {
            if (ApplicationLanguages.PrimaryLanguageOverride == value.Name)
                return;
            ApplicationLanguages.PrimaryLanguageOverride = value.Name;
            OnPropertyChanged();
        }
    }

    public void ResetDefault()
    {
        AppSetting = new() { LastCheckedUpdate = AppSetting.LastCheckedUpdate };
        PixivAppApiNameResolver = [.. AppSetting.PixivAppApiNameResolver];
        PixivImageNameResolver = [.. AppSetting.PixivImageNameResolver];
        PixivImageNameResolver2 = [.. AppSetting.PixivImageNameResolver2];
        PixivOAuthNameResolver = [.. AppSetting.PixivOAuthNameResolver];
        PixivAccountNameResolver = [.. AppSetting.PixivAccountNameResolver];
        PixivWebApiNameResolver = [.. AppSetting.PixivWebApiNameResolver];

        // see OnPropertyChanged
        OnPropertyChanged(nameof(DisableDomainFronting));
        OnPropertyChanged(nameof(MirrorHost));
        OnPropertyChanged(nameof(MaxDownloadTaskConcurrencyLevel));
        AppLanguage = LanguageModel.DefaultLanguage;
    }

    public DateTimeOffset GetMinSearchEndDate(DateTimeOffset startDate)
    {
        return startDate.AddDays(1);
    }

    public string GetLastUpdateCheckDisplayString(DateTimeOffset lastChecked)
    {
        return SettingsPageResources.LastCheckedPrefix + lastChecked.ToString(CultureInfo.CurrentUICulture);
    }

    public void ShowClearData(ClearDataKind kind)
    {
        FrameworkElement.ShowTeachingTipAndHide(kind.GetLocalizedResourceContent()!);
    }

    public void SaveCollections()
    {
        var appApiNameSame = AppSetting.PixivAppApiNameResolver.SequenceEquals(PixivAppApiNameResolver);
        var imageNameSame = AppSetting.PixivImageNameResolver.SequenceEqual(PixivImageNameResolver);
        var imageName2Same = AppSetting.PixivImageNameResolver2.SequenceEqual(PixivImageNameResolver2);
        var oAuthNameSame = AppSetting.PixivOAuthNameResolver.SequenceEqual(PixivOAuthNameResolver);
        var accountNameSame = AppSetting.PixivAccountNameResolver.SequenceEqual(PixivAccountNameResolver);
        var webApiNameSame = AppSetting.PixivWebApiNameResolver.SequenceEqual(PixivWebApiNameResolver);

        AppSetting.PixivAppApiNameResolver = [.. PixivAppApiNameResolver];
        AppSetting.PixivImageNameResolver = [.. PixivImageNameResolver];
        AppSetting.PixivImageNameResolver2 = [.. PixivImageNameResolver2];
        AppSetting.PixivOAuthNameResolver = [.. PixivOAuthNameResolver];
        AppSetting.PixivAccountNameResolver = [.. PixivAccountNameResolver];
        AppSetting.PixivWebApiNameResolver = [.. PixivWebApiNameResolver];

        if (appApiNameSame || imageNameSame || imageName2Same || oAuthNameSame || accountNameSame || webApiNameSame)
            AppInfo.SetNameResolvers(AppSetting);
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

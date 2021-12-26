#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/SettingsPageViewModel.cs
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
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Download;
using Pixeval.Download.MacroParser;
using Pixeval.Misc;
using Pixeval.Options;
using Pixeval.UserControls;
using Pixeval.UserControls.TokenInput;

namespace Pixeval.Controls.Setting.UI;

public class SettingsPageViewModel : ObservableObject
{
    public static readonly IEnumerable<string> AvailableFonts = new InstalledFontCollection().Families.Select(f => f.Name);

    public static readonly ICollection<Token> AvailableIllustMacros;

    private readonly AppSetting _appSetting;

    static SettingsPageViewModel()
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var factory = scope.ServiceProvider.GetRequiredService<IDownloadTaskFactory<IllustrationViewModel, ObservableDownloadTask>>();
        AvailableIllustMacros = factory.PathParser.MacroProvider.AvailableMacros
            .Select(m => $"@{{{(m is IMacro<IllustrationViewModel>.IPredicate ? $"{m.Name}:" : m.Name)}}}")
            .Select(s => new Token(s, false, false))
            .ToList();
    }

    public SettingsPageViewModel(AppSetting appSetting)
    {
        _appSetting = appSetting;
    }

    [DefaultValue(ApplicationTheme.SystemDefault)]
    public ApplicationTheme Theme
    {
        get => _appSetting.Theme;
        set => SetProperty(_appSetting.Theme, value, _appSetting, (setting, value) => setting.Theme = value);
    }

    [DefaultValue(false)]
    public bool FiltrateRestrictedContent
    {
        get => _appSetting.FiltrateRestrictedContent;
        set => SetProperty(_appSetting.FiltrateRestrictedContent, value, _appSetting, (setting, value) => setting.FiltrateRestrictedContent = value);
    }

    [DefaultValue(false)]
    public bool DisableDomainFronting
    {
        get => _appSetting.DisableDomainFronting;
        set => SetProperty(_appSetting.DisableDomainFronting, value, _appSetting, (setting, value) =>
        {
            setting.DisableDomainFronting = value;
            App.AppViewModel.MakoClient.Configuration.Bypass = !value;
        });
    }

    [DefaultValue(IllustrationSortOption.DoNotSort)]
    public IllustrationSortOption DefaultSortOption
    {
        get => _appSetting.DefaultSortOption;
        set => SetProperty(_appSetting.DefaultSortOption, value, _appSetting, (setting, value) => setting.DefaultSortOption = value);
    }

    [DefaultValue(SearchTagMatchOption.PartialMatchForTags)]
    public SearchTagMatchOption TagMatchOption
    {
        get => _appSetting.TagMatchOption;
        set => SetProperty(_appSetting.TagMatchOption, value, _appSetting, (setting, value) => setting.TagMatchOption = value);
    }

    [DefaultValue(TargetFilter.ForAndroid)]
    public TargetFilter TargetFilter
    {
        get => _appSetting.TargetFilter;
        set => SetProperty(_appSetting.TargetFilter, value, _appSetting, (setting, value) => setting.TargetFilter = value);
    }

    [DefaultValue(2)]
    public int PreLoadRows
    {
        get => _appSetting.PreLoadRows;
        set => SetProperty(_appSetting.PreLoadRows, value, _appSetting, (setting, value) => setting.PreLoadRows = value);
    }

    [DefaultValue(100)]
    public int PageLimitForKeywordSearch
    {
        get => _appSetting.PageLimitForKeywordSearch;
        set => SetProperty(_appSetting.PageLimitForKeywordSearch, value, _appSetting, (setting, value) => setting.PageLimitForKeywordSearch = value);
    }

    [DefaultValue(1)]
    public int SearchStartingFromPageNumber
    {
        get => _appSetting.SearchStartingFromPageNumber;
        set => SetProperty(_appSetting.SearchStartingFromPageNumber, value, _appSetting, (setting, value) => setting.SearchStartingFromPageNumber = value);
    }

    [DefaultValue(50)]
    public int PageLimitForSpotlight
    {
        get => _appSetting.PageLimitForSpotlight;
        set => SetProperty(_appSetting.PageLimitForSpotlight, value, _appSetting, (setting, value) => setting.PageLimitForSpotlight = value);
    }

    [DefaultValue(null)]
    public string? MirrorHost
    {
        get => _appSetting.MirrorHost;
        set => SetProperty(_appSetting.MirrorHost, value, _appSetting, (setting, value) =>
        {
            setting.MirrorHost = value;
            App.AppViewModel.MakoClient.Configuration.MirrorHost = value;
        });
    }

    [DefaultValue(typeof(DownloadConcurrencyDefaultValueProvider))]
    public int MaxDownloadTaskConcurrencyLevel
    {
        get => _appSetting.MaxDownloadTaskConcurrencyLevel;
        set => SetProperty(_appSetting.MaxDownloadTaskConcurrencyLevel, value, _appSetting, (setting, value) =>
        {
            App.AppViewModel.DownloadManager.ConcurrencyDegree = value;
            setting.MaxDownloadTaskConcurrencyLevel = value;
        });
    }

    [DefaultValue(true)]
    public bool DisplayTeachingTipWhenGeneratingAppLink
    {
        get => _appSetting.DisplayTeachingTipWhenGeneratingAppLink;
        set => SetProperty(_appSetting.DisplayTeachingTipWhenGeneratingAppLink, value, _appSetting, (setting, value) => setting.DisplayTeachingTipWhenGeneratingAppLink = value);
    }

    [DefaultValue(500)]
    public int ItemsNumberLimitForDailyRecommendations
    {
        get => _appSetting.ItemsNumberLimitForDailyRecommendations;
        set => SetProperty(_appSetting.ItemsNumberLimitForDailyRecommendations, value, _appSetting, (settings, value) => settings.ItemsNumberLimitForDailyRecommendations = value);
    }

    [DefaultValue(false)]
    public bool UseFileCache
    {
        get => _appSetting.UseFileCache;
        set => SetProperty(_appSetting.UseFileCache, value, _appSetting, (settings, value) => settings.UseFileCache = value);
    }

    [DefaultValue(ThumbnailDirection.Portrait)]
    public ThumbnailDirection ThumbnailDirection
    {
        get => _appSetting.ThumbnailDirection;
        set => SetProperty(_appSetting.ThumbnailDirection, value, _appSetting, (settings, value) => settings.ThumbnailDirection = value);
    }

    [DefaultValue(typeof(MinDateTimeOffSetDefaultValueProvider))]
    public DateTimeOffset LastCheckedUpdate
    {
        get => _appSetting.LastCheckedUpdate;
        set => SetProperty(_appSetting.LastCheckedUpdate, value, _appSetting, (settings, value) => settings.LastCheckedUpdate = value);
    }

    [DefaultValue(false)]
    public bool DownloadUpdateAutomatically
    {
        get => _appSetting.DownloadUpdateAutomatically;
        set => SetProperty(_appSetting.DownloadUpdateAutomatically, value, _appSetting, (settings, value) => settings.DownloadUpdateAutomatically = value);
    }

    [DefaultValue("Segoe UI")]
    public string AppFontFamilyName
    {
        get => _appSetting.AppFontFamilyName;
        set => SetProperty(_appSetting.AppFontFamilyName, value, _appSetting, (settings, value) => settings.AppFontFamilyName = value);
    }

    [DefaultValue(MainPageTabItem.DailyRecommendation)]
    public MainPageTabItem DefaultSelectedTabItem
    {
        get => _appSetting.DefaultSelectedTabItem;
        set => SetProperty(_appSetting.DefaultSelectedTabItem, value, _appSetting, (settings, value) => settings.DefaultSelectedTabItem = value);
    }

    [DefaultValue(SearchDuration.Undecided)]
    public SearchDuration SearchDuration
    {
        get => _appSetting.SearchDuration;
        set => SetProperty(_appSetting.SearchDuration, value, _appSetting, (settings, value) => settings.SearchDuration = value);
    }

    [DefaultValue(false)]
    public bool UsePreciseRangeForSearch
    {
        get => _appSetting.UsePreciseRangeForSearch;
        set => SetProperty(_appSetting.UsePreciseRangeForSearch, value, _appSetting, (settings, value) => settings.UsePreciseRangeForSearch = value);
    }

    [DefaultValue(typeof(DecrementedDateTimeOffSetDefaultValueProvider))]
    public DateTimeOffset SearchStartDate
    {
        get => _appSetting.SearchStartDate;
        set => SetProperty(_appSetting.SearchStartDate, value, _appSetting, (settings, value) => settings.SearchStartDate = value);
    }

    [DefaultValue(typeof(CurrentDateTimeOffSetDefaultValueProvider))]
    public DateTimeOffset SearchEndDate
    {
        get => _appSetting.SearchEndDate;
        set => SetProperty(_appSetting.SearchEndDate, value, _appSetting, (settings, value) => settings.SearchEndDate = value);
    }

    [DefaultValue(typeof(DownloadPathMacroDefaultValueProvider))]
    public string DefaultDownloadPathMacro
    {
        get => _appSetting.DefaultDownloadPathMacro;
        set => SetProperty(_appSetting.DefaultDownloadPathMacro, value, _appSetting, (settings, value) => settings.DefaultDownloadPathMacro = value);
    }

    [DefaultValue(false)]
    public bool OverwriteDownloadedFile
    {
        get => _appSetting.OverwriteDownloadedFile;
        set => SetProperty(_appSetting.OverwriteDownloadedFile, value, _appSetting, (settings, value) => settings.OverwriteDownloadedFile = value);
    }

    [DefaultValue(100)]
    public int MaximumDownloadHistoryRecords
    {
        get => _appSetting.MaximumDownloadHistoryRecords;
        set => SetProperty(_appSetting.MaximumDownloadHistoryRecords, value, _appSetting, (settings, value) => settings.MaximumDownloadHistoryRecords = value);
    }

    [DefaultValue(50)]
    public int MaximumSearchHistoryRecords
    {
        get => _appSetting.MaximumSearchHistoryRecords;
        set => SetProperty(_appSetting.MaximumSearchHistoryRecords, value, _appSetting, (settings, value) => settings.MaximumSearchHistoryRecords = value);
    }

    [DefaultValue(100)]
    public int MaximumBrowseHistoryRecords
    {
        get => _appSetting.MaximumBrowseHistoryRecords;
        set => SetProperty(_appSetting.MaximumBrowseHistoryRecords, value, _appSetting, (settings, value) => settings.MaximumBrowseHistoryRecords = value);
    }

    [DefaultValue(null)]
    public string? ReverseSearchApiKey
    {
        get => _appSetting.ReverseSearchApiKey;
        set => SetProperty(_appSetting.ReverseSearchApiKey, value, _appSetting, (settings, value) => settings.ReverseSearchApiKey = value);
    }

    [DefaultValue(80)]
    public int ReverseSearchResultSimilarityThreshold
    {
        get => _appSetting.ReverseSearchResultSimilarityThreshold;
        set => SetProperty(_appSetting.ReverseSearchResultSimilarityThreshold, value, _appSetting, (settings, value) => settings.ReverseSearchResultSimilarityThreshold = value);
    }

    public DateTimeOffset GetMinSearchEndDate(DateTimeOffset startDate)
    {
        return startDate.AddDays(1);
    }

    public string GetLastUpdateCheckDisplayString(DateTimeOffset lastChecked)
    {
        return $"{SettingsPageResources.LastCheckedPrefix}{lastChecked.ToString(CultureInfo.CurrentUICulture)}";
    }

    public void ResetDefault()
    {
        foreach (var propertyInfo in typeof(SettingsPageViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            propertyInfo.SetValue(this, propertyInfo.GetDefaultValue());
        }
    }
}
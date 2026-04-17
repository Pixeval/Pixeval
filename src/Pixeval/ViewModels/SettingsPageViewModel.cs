// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using AutoSettingsPage;
using AutoSettingsPage.Avalonia;
using AutoSettingsPage.Models;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.AppManagement;
using Pixeval.Models.Extensions;
using Pixeval.Models.Options;
using Pixeval.Models.Settings;
using Pixeval.Utilities;

namespace Pixeval.ViewModels;

public class SettingsPageViewModel : ViewModelBase
{
    public string CurrentVersion =>
        AppInfo.AppVersion.CurrentVersion.Let(t => $"{t.Major}.{t.Minor}.{t.Build}.{t.Revision}");

    public DateTime LastCheckedUpdate
    {
        get => AppSettings.LastCheckedUpdate;
        set => SetProperty(AppSettings.LastCheckedUpdate, value, AppSettings, (setting, v) => setting.LastCheckedUpdate = v);
    }

    public AppSettings AppSettings => App.AppViewModel.AppSettings;

    public IEnumerable<ISettingsGroup> Groups => LocalGroups.Concat(ExtensionGroups);

    public IReadOnlyList<ISettingsGroup> LocalGroups { get; } = SettingsBuilder.CreateGroupList(App.AppViewModel.AppSettings)
            .NewGroup(SettingsEntryCategory.Application)
            .Config(group => group
                .Language(t => t.CultureName)
                .Enum(t => t.Theme, ApplicationThemeExtension.Items,
                    entry => entry.ValueChanged += t => Application.Current?.RequestedThemeVariant = t switch
                    {
                        ApplicationTheme.Light => ThemeVariant.Light,
                        ApplicationTheme.Dark => ThemeVariant.Dark,
                        _ => ThemeVariant.Default
                    })
                .Font(t => t.AppFontFamily, entry => entry.ValueChanged += t => Application.Current?.Resources["ContentControlThemeFontFamily"] = new FontFamily(t))
                .DomainFronting(t => t.EnableDomainFronting, entry =>
                        entry.IPSet(t => t.PixivAppApiNameResolver)
                            .IPSet(t => t.PixivImageNameResolver)
                            .IPSet(t => t.PixivImageNameResolver2)
                            .IPSet(t => t.PixivOAuthNameResolver)
                            .IPSet(t => t.PixivAccountNameResolver)
                            .IPSet(t => t.PixivWebApiNameResolver),
                    entry => entry.ValueChanged += t => App.AppViewModel.MakoClient.Configuration.DomainFronting = t)
                .Proxy(entry => entry.ProxyChanged += t => App.AppViewModel.MakoClient.Configuration.Proxy = t)
                .String(t => t.MirrorHost,
                    entry => entry.ValueChanged += t => App.AppViewModel.MakoClient.Configuration.MirrorHost = t)
                .Bool(t => t.UseFileCache)
                .Enum(t => t.DefaultSelectedTabItem, MainPageTabItemExtension.Items)
                .String(t => t.WebCookie,
                    entry => entry.ValueChanged += t => App.AppViewModel.MakoClient.Configuration.Cookie = t))
            .NewGroup(SettingsEntryCategory.BrowsingExperience)
            .Config(group => group
                .Enum(t => t.ThumbnailDirection, ThumbnailDirectionExtension.Items)
                .Enum(t => t.ItemsViewLayoutType, ItemsViewLayoutTypeExtension.Items)
                .Enum(t => t.TargetFilter, TargetFilterExtension.Items)
                .Collection(t => t.BlockedTags)
                .Bool(t => t.OpenWorkInfoByDefault))
            .NewGroup(SettingsEntryCategory.Search)
            .Config(group => group
                .String(t => t.ReverseSearchApiKey,
                    entry => entry.DescriptionUri = new("https://saucenao.com/user.php?page=search-api"))
                .Int(t => t.ReverseSearchResultSimilarityThreshold, 1, 100, 1)
                .Int(t => t.MaximumSearchHistoryRecords, 10, 200, 1)
                .Int(t => t.MaximumSuggestionBoxSearchHistory, 0, 20, 1)
                .Enum(t => t.WorkSortOption, WorkSortOptionExtension.Items)
                .Enum(t => t.SimpleWorkType, SimpleWorkTypeExtension.Items)
                .MultiValues(t => t.IllustrationRankOption, entry =>
                    entry.Enum(
                            WorkTypeEnum.Illustration,
                            t => t.IllustrationRankOption,
                            RankOptionExtension.IllustrationItems)
                        .Enum(
                            WorkTypeEnum.Novel,
                            t => t.NovelRankOption,
                            RankOptionExtension.NovelItems))
                .MultiValues(t => t.SearchIllustrationTagMatchOption, entry =>
                    entry.Enum(
                            WorkTypeEnum.Illustration,
                            t => t.SearchIllustrationTagMatchOption,
                            SearchIllustrationTagMatchOptionExtension.Items)
                        .Enum(
                            WorkTypeEnum.Novel,
                            t => t.SearchNovelTagMatchOption,
                            SearchNovelTagMatchOptionExtension.Items))
                .DateWithSwitch(t => t.UseSearchStartDate,
                    entry => entry.DateTime(t => t.SearchStartDate, DateTime.MinValue,
                        DateTime.MaxValue))
                .DateWithSwitch(t => t.UseSearchEndDate,
                    entry => entry.DateTime(t => t.SearchEndDate, DateTime.MinValue,
                        DateTime.MaxValue)))
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
                            IllustrationDownloadFormatExtension.Items)
                        .Enum(
                            WorkTypeEnum.Ugoira,
                            t => t.UgoiraDownloadFormat,
                            UgoiraDownloadFormatExtension.Items)
                        .Enum(
                            WorkTypeEnum.Novel,
                            t => t.NovelDownloadFormat,
                            NovelDownloadFormatExtension.Items))
                .Int(t => t.LossyImageDownloadQuality, -1, 100, 5)
                .Bool(t => t.DownloadWhenBookmarked))
            .NewGroup(SettingsEntryCategory.Misc)
            .Config(group => group
                .Int(t => t.MaximumBrowseHistoryRecords, 10, ushort.MaxValue, 10))
            .Build();

    public IReadOnlyList<ExtensionSettingsGroup> ExtensionGroups { get; } =
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>().SettingsGroups;
}

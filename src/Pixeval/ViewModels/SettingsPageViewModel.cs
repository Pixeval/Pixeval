// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using AutoSettingsPage;
using AutoSettingsPage.Models;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Mako;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.AppManagement;
using Pixeval.Models.Extensions;
using Pixeval.Models.Options;
using Pixeval.Models.Settings;

namespace Pixeval.ViewModels;

public class SettingsPageViewModel : ViewModelBase
{
    public string CurrentVersion => AppInfo.AppVersion.CurrentVersionText;

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
                .Enum(t => t.Theme,
                    entry => entry.ValueChanged += t => Application.Current?.RequestedThemeVariant = t switch
                    {
                        ApplicationTheme.Light => ThemeVariant.Light,
                        ApplicationTheme.Dark => ThemeVariant.Dark,
                        _ => ThemeVariant.Default
                    })
                .Font(t => t.AppFontFamily, entry => entry.ValueChanged += t => Application.Current?.Resources["ContentControlThemeFontFamily"] = new FontFamily(string.Join(',', t)))
                .Bool(t => t.UseFileCache)
                .Enum(t => t.DefaultSelectedTabItem))
            .NewGroup(SettingsEntryCategory.Network)
            .Config(group => group
                .DomainFronting(t => t.EnableDomainFronting, entry =>
                        entry.Enum(t => t.DomainFrontingType,
                                e => e.ValueChanged += t => App.AppViewModel.MakoClient.Configuration.DomainFrontingType = (DomainFrontingType) t)
                            .IPSet(t => t.PixivAppApiNameResolver)
                            .IPSet(t => t.PixivImageNameResolver)
                            .IPSet(t => t.PixivImageNameResolver2)
                            .IPSet(t => t.PixivOAuthNameResolver)
                            .IPSet(t => t.PixivAccountNameResolver)
                            .IPSet(t => t.PixivWebApiNameResolver),
                    entry => entry.ValueChanged += t => App.AppViewModel.MakoClient.Configuration.DomainFronting = t)
                .Proxy(entry => entry.ProxyChanged += t => App.AppViewModel.MakoClient.Configuration.Proxy = t)
                .String(t => t.MirrorHost,
                    entry => entry.ValueChanged += t => App.AppViewModel.MakoClient.Configuration.MirrorHost = t)
                .String(t => t.WebCookie,
                    entry => entry.ValueChanged += t => App.AppViewModel.MakoClient.Configuration.Cookie = t))
            .NewGroup(SettingsEntryCategory.BrowsingExperience)
            .Config(group => group
                .Enum(t => t.ThumbnailLayoutType)
                .Enum(t => t.BrowseMode)
                .Enum(t => t.BrowseDirection)
                .Enum(t => t.TargetFilter)
                .Collection(t => t.BlockedTags)
                .Bool(t => t.OpenWorkInfoByDefault))
            .NewGroup(SettingsEntryCategory.Search)
            .Config(group => group
                .String(t => t.ReverseSearchApiKey,
                    entry => entry.DescriptionUri = new("https://saucenao.com/user.php?page=search-api"))
                .Int(t => t.ReverseSearchResultSimilarityThreshold, 1, 100, 1)
                .Int(t => t.MaximumSuggestionBoxSearchHistory, 0, 20, 1)
                .Enum(t => t.LocalSortOption)
                .Enum(t => t.SimpleWorkType)
                .MultiValues(t => t.IllustrationRankOption, entry =>
                    entry.Enum(
                            WorkTypeEnum.Illustration,
                            t => t.IllustrationRankOption,
                            "Illustration")
                        .Enum(
                            WorkTypeEnum.Novel,
                            t => t.NovelRankOption,
                            "Novel"))
                .MultiValues(t => t.SearchIllustrationTagMatchOption, entry =>
                    entry.Enum(
                            WorkTypeEnum.Illustration,
                            t => t.SearchIllustrationTagMatchOption)
                        .Enum(
                            WorkTypeEnum.Novel,
                            t => t.SearchNovelTagMatchOption))
                .DateWithSwitch(t => t.UseSearchStartDate,
                    entry => entry.DateTime(t => t.SearchStartDate, DateTime.MinValue,
                        DateTime.MaxValue))
                .DateWithSwitch(t => t.UseSearchEndDate,
                    entry => entry.DateTime(t => t.SearchEndDate, DateTime.MinValue,
                        DateTime.MaxValue)))
            .NewGroup(SettingsEntryCategory.Download)
            .Config(group => group
                .Bool(t => t.OverwriteDownloadedFile)
                .Int(t => t.MaxDownloadTaskConcurrencyLevel, 1, Environment.ProcessorCount, 1,
                    entry => entry.ValueChanged += t => App.AppViewModel.HistoryPersistHelper.DownloadManager.ConcurrencyDegree = t)
                .DownloadMacro()
                .MultiValues(t => t.IllustrationDownloadFormat, entry =>
                    entry.IllustrationDownloadFormat(WorkTypeEnum.Illustration)
                        .UgoiraDownloadFormat(WorkTypeEnum.Ugoira)
                        .NovelDownloadFormat(WorkTypeEnum.Novel))
                .WorkSubscriptions())
            //.NewGroup(SettingsEntryCategory.Misc)
            //.Config(group => group)
            .Build();

    public IReadOnlyList<ExtensionSettingsGroup> ExtensionGroups { get; } =
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>().SettingsGroups;
}

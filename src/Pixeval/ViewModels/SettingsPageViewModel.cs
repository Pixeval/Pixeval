// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoSettingsPage;
using AutoSettingsPage.Models;
using Avalonia;
using Avalonia.Styling;
using Mako;
using Mako.Global.Enum;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.AppManagement;
using Pixeval.Models.Extensions;
using Pixeval.Models.Options;
using Pixeval.Models.Settings;
using Pixeval.Utilities;
using Pixeval.Utilities.IO.Caching;

namespace Pixeval.ViewModels;

public class SettingsPageViewModel : ViewModelBase
{
    public string CurrentVersion => AppInfo.AppVersion.CurrentVersionShortText;

    public DateTime LastCheckedUpdate
    {
        get => AppSettings.ApplicationSettings.LastCheckedUpdate;
        set => SetProperty(AppSettings.ApplicationSettings.LastCheckedUpdate, value, AppSettings.ApplicationSettings, (setting, v) => setting.LastCheckedUpdate = v);
    }

    public AppSettings AppSettings => App.AppViewModel.AppSettings;

    public IEnumerable<ISettingsGroup> Groups => LocalGroups.Concat(ExtensionGroups);

    public IReadOnlyList<ISettingsGroup> LocalGroups { get; } = BuildLocalGroups();

    public IReadOnlyList<ExtensionSettingsGroup> ExtensionGroups { get; } =
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>().SettingsGroups;

    private static IReadOnlyList<ISettingsGroup> BuildLocalGroups()
    {
        LocalSettingsEntryHelper.Initialize();

        return SettingsBuilder.CreateGroupList(App.AppViewModel.AppSettings)
            .NewGroup(t => t.ApplicationSettings, group => group
                .Language(t => t.CultureName)
                .Enum(t => t.Theme,
                    entry => entry.ValueChanged += t => Application.Current?.RequestedThemeVariant = t switch
                    {
                        ApplicationTheme.Light => ThemeVariant.Light,
                        ApplicationTheme.Dark => ThemeVariant.Dark,
                        _ => ThemeVariant.Default
                    })
                .Font(t => t.AppFontFamily, entry => entry.ValueChanged += App.ApplyAppFontFamily)
                .Bool(t => t.UseFileCache)
                .MultiValuesWithSwitch(t => t.LimitFileCacheSize,
                    entry => entry.Int(t => t.FileCacheSizeLimitInMegabytes, 1, 0x100000, 0x80,
                        t => t.ValueChanged += _ => CacheHelper.EnforceCacheSizeLimit()),
                    t => t.ValueChanged += enabled =>
                    {
                        if (enabled)
                            CacheHelper.EnforceCacheSizeLimit();
                    })
                .Int(t => t.HomePageRows, 1, 12, 1)
                .Int(t => t.HomePageColumns, 1, 12, 1)
                .Bool(t => t.HideHomePageToolbar)
                .Bool(t => t.HideHomePageCardTitle))
            .NewGroup(t => t.NetworkSettings, group => group
                .DomainFronting(t => t.EnablePixivDomainFronting, entry =>
                        entry.Enum(t => t.PixivDomainFrontingType,
                                e => e.ValueChanged += t => App.AppViewModel.MakoClient.Configuration.DomainFrontingType = (DomainFrontingType) t)
                            .IPSet(t => t.PixivAppApiNameResolver)
                            .IPSet(t => t.PixivImageNameResolver)
                            .IPSet(t => t.PixivImageNameResolver2)
                            .IPSet(t => t.PixivOAuthNameResolver)
                            .IPSet(t => t.PixivAccountNameResolver)
                            .IPSet(t => t.PixivWebApiNameResolver),
                    entry => entry.ValueChanged += t => App.AppViewModel.MakoClient.Configuration.DomainFronting = t)
                .DomainFronting(t => t.EnableGitHubDomainFronting, entry => entry
                    .IPSet(t => t.GitHubNameResolver)
                    .IPSet(t => t.GitHubApiNameResolver)
                    .IPSet(t => t.GitHubAvatarNameResolver)
                    .IPSet(t => t.GitHubUserContentNameResolver)
                    .IPSet(t => t.GitHubAssetsNameResolver)
                    .IPSet(t => t.GitHubCodeloadNameResolver))
                .Proxy(entry => entry.ProxyChanged += t => App.AppViewModel.MakoClient.Configuration.Proxy = t)
                .String(t => t.MirrorHost,
                    entry => entry.ValueChanged += t => App.AppViewModel.MakoClient.Configuration.MirrorHost = t)
                .String(t => t.WebCookie,
                    entry => entry.ValueChanged += t => App.AppViewModel.MakoClient.Configuration.Cookie = t))
            .NewGroup(t => t.BrowsingExperienceSettings, group => group
                .Enum(t => t.ThumbnailLayoutType)
                .Enum(t => t.BrowseMode)
                .Enum(t => t.BrowseDirection)
                .Int(t => t.IllustrationViewerAutoPlayInterval, 1, 60, 1)
                .Enum(t => t.IllustrationViewerAutoPlayMode)
                .Enum(t => t.IllustrationViewerAutoPlayScope)
                .Enum(t => t.TargetFilter)
                .Collection(t => t.BlockedTags)
                .Bool(t => t.OpenWorkInfoByDefault)
                .Bool(t => t.OpenUserInfoByDefault))
            .NewGroup(t => t.SearchSettings, group => group
                .String(t => t.SauceNaoApiKey)
                .Enum(t => t.DefaultSimpleWorkType)
                .MultiValues(t => t.IllustrationRankOption, entry =>
                    entry.Enum(
                            WorkTypeEnum.Illustration,
                            t => t.IllustrationRankOption,
                            SimpleWorkType.Illustration)
                        .Enum(
                            WorkTypeEnum.Novel,
                            t => t.NovelRankOption,
                            SimpleWorkType.Novel)))
            .NewGroup(t => t.DownloadSettings, group => group
                .Bool(t => t.OverwriteDownloadedFile)
                .Int(t => t.MaxDownloadTaskConcurrencyLevel, 1, Environment.ProcessorCount, 1,
                    entry => entry.ValueChanged += t => App.AppViewModel.HistoryPersistHelper.DownloadManager.ConcurrencyDegree = t)
                .DownloadMacro(t => t.DownloadPathMacro)
                .MultiValues(t => t.IllustrationDownloadFormat, entry =>
                    entry.IllustrationDownloadFormat()
                        .UgoiraDownloadFormat()
                        .NovelDownloadFormat())
                .WorkSubscriptions(t => t.WorkSubscriptions))
            .NewGroup(t => t.McpSettings, group => group
                .Bool(t => t.EnableServer, entry => entry.ValueChanged += value =>
                {
                    _ = ApplyMcpSettingsAsync();
                })
                .Int(t => t.Port, 1, 65535, 1, entry => entry.ValueChanged += value =>
                {
                    _ = ApplyMcpSettingsAsync();
                })
                .Bool(t => t.EnableWriteTools)
                .Int(t => t.MaxBinaryResourceMegabytes, 1, McpSettingsGroup.MaxBinaryResourceMegabytesLimit, 1))
            .Build();
    }

    private static async Task ApplyMcpSettingsAsync()
    {
        try
        {
            if (App.AppViewModel.AppServiceProvider.GetService<IPixevalMcpService>() is { } service)
                await service.ApplySettingsAsync();
        }
        catch (Exception e)
        {
            App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                .LogError("Failed to apply Pixeval MCP settings", e);
        }
    }
}

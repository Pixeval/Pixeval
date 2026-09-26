// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using AutoSettingsPage;
using Avalonia;
using Avalonia.Styling;
using FluentIcons.Common;
using Mako;
using Pixeval.Models.Options;
using Pixeval.Utilities;

namespace Pixeval.AppManagement.Settings;

public record AppSettings
{
    public void Initialize()
    {
        if (NetworkSettings.GitHubDomainFronting.GitHubAssetsNameResolver is
            [
                "185.199.108.154",
                "185.199.109.154",
                "185.199.110.154",
                "185.199.111.154"
            ])
        {
            NetworkSettings.GitHubDomainFronting.GitHubAssetsNameResolver = new NetworkSettingsGroup().GitHubDomainFronting.GitHubAssetsNameResolver;
        }

        if (LastOpenedVersion != AppInfo.AppVersion.CurrentVersionShortText)
        {
            IsNewVersion = true;
            LastOpenedVersion = AppInfo.AppVersion.CurrentVersionShortText;

            // 更新时若域前置IP改变，则在此手动更新域名
            //var network = new NetworkSettingsGroup();
            //NetworkSettings.PixivDomainFronting.PixivAppApiNameResolver = network.PixivDomainFronting.PixivAppApiNameResolver;
            //NetworkSettings.PixivDomainFronting.PixivWebApiNameResolver = network.PixivDomainFronting.PixivWebApiNameResolver;
            //NetworkSettings.PixivDomainFronting.PixivAccountNameResolver = network.PixivDomainFronting.PixivAccountNameResolver;
            //NetworkSettings.PixivDomainFronting.PixivOAuthNameResolver = network.PixivDomainFronting.PixivOAuthNameResolver;
            //NetworkSettings.PixivDomainFronting.PixivImageNameResolver = network.PixivDomainFronting.PixivImageNameResolver;
            //NetworkSettings.PixivDomainFronting.PixivImageNameResolver2 = network.PixivDomainFronting.PixivImageNameResolver2;

            //NetworkSettings.GitHubDomainFronting.GitHubNameResolver = network.GitHubDomainFronting.GitHubNameResolver;
            //NetworkSettings.GitHubDomainFronting.GitHubApiNameResolver = network.GitHubDomainFronting.GitHubApiNameResolver;
            //NetworkSettings.GitHubDomainFronting.GitHubAvatarNameResolver = network.GitHubDomainFronting.GitHubAvatarNameResolver;
            //NetworkSettings.GitHubDomainFronting.GitHubUserContentNameResolver = network.GitHubDomainFronting.GitHubUserContentNameResolver;
            //NetworkSettings.GitHubDomainFronting.GitHubAssetsNameResolver = network.GitHubDomainFronting.GitHubAssetsNameResolver;
            //NetworkSettings.GitHubDomainFronting.GitHubCodeloadNameResolver = network.GitHubDomainFronting.GitHubCodeloadNameResolver;
        }
    }

    [SettingsEntry(Symbol.Apps, AppSettingsResources.SettingsGroup.Application.Header, null)]
    public ApplicationSettingsGroup ApplicationSettings { get; set; } = new();

    [SettingsEntry(Symbol.WiFi, AppSettingsResources.SettingsGroup.Network.Header, null)]
    public NetworkSettingsGroup NetworkSettings { get; set; } = new();

    [SettingsEntry(Symbol.News, AppSettingsResources.SettingsGroup.BrowsingExperience.Header, null)]
    public BrowsingExperienceSettingsGroup BrowsingExperienceSettings { get; set; } = new();

    [SettingsEntry(Symbol.SearchSparkle, AppSettingsResources.SettingsGroup.Search.Header, null)]
    public SearchSettingsGroup SearchSettings { get; set; } = new();

    [SettingsEntry(Symbol.ArrowSquareDown, AppSettingsResources.SettingsGroup.Download.Header, null)]
    public DownloadSettingsGroup DownloadSettings { get; set; } = new();

#if PIXEVAL_MCP
    [SettingsEntry(Symbol.Bot, AppSettingsResources.SettingsGroup.Mcp.Header, null)]
    public McpSettingsGroup McpSettings { get; set; } = new();
#endif

    [SettingsEntry(Symbol.Settings, AppSettingsResources.SettingsGroup.Novel.Header, null)]
    public NovelSettingsGroup NovelSettings { get; set; } = new();

    /// <summary>
    /// <see cref="object"/> 只能是基元类型或 <see cref="SettingsSerializerContext"/> 提供过的类型
    /// </summary>
    public Dictionary<string, Dictionary<string, object?>> ExtensionSettings { get; set; } = [];

    /// <summary>
    /// 相对于 <see cref="AppInfo.ExtensionsFolder"/> 的待卸载扩展文件或目录路径。
    /// </summary>
    public HashSet<string> PendingExtensionUninstallTargets { get; set; } = [];

    public string LastOpenedVersion { get; set; } = "";

    [JsonIgnore]
    public bool IsNewVersion { get; private set; }

    [JsonIgnore]
    public ApplicationTheme ActualTheme => ApplicationSettings.Theme is ApplicationTheme.Default
        ? Application.Current!.ActualThemeVariant == ThemeVariant.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light
        : ApplicationSettings.Theme;

    public MakoConfiguration ToMakoConfiguration()
    {
        return new MakoConfiguration(
            NetworkSettings.PixivDomainFronting.EnablePixivDomainFronting,
            NetworkSettings.PixivDomainFronting.PixivDomainFrontingType,
            MakoHelper.ToMakoProxy(NetworkSettings.ProxySettings.ProxyType, NetworkSettings.ProxySettings.Proxy),
            NetworkSettings.WebCookie,
            NetworkSettings.MirrorHost,
            BrowsingExperienceSettings.TargetFilter,
            NetworkSettings.ApiRequestCooldown,
            CultureInfo.CurrentCulture);
    }
}

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
using static Pixeval.AppSettingsResources;

namespace Pixeval.AppManagement;

public record AppSettings
{
    public AppSettings()
    {
        NovelSettings.ActualThemeProvider = () => ActualTheme;
    }

    public void Initialize()
    {
        if (LastOpenedVersion != AppInfo.AppVersion.CurrentVersionShortText)
        {
            IsNewVersion = true;
            LastOpenedVersion = AppInfo.AppVersion.CurrentVersionShortText;

            // 更新时若域前置IP改变，则在此手动更新域名
            //var network = new NetworkSettingsGroup();
            //NetworkSettings.PixivAppApiNameResolver = network.PixivAppApiNameResolver;
            //NetworkSettings.PixivWebApiNameResolver = network.PixivWebApiNameResolver;
            //NetworkSettings.PixivAccountNameResolver = network.PixivAccountNameResolver;
            //NetworkSettings.PixivOAuthNameResolver = network.PixivOAuthNameResolver;
            //NetworkSettings.PixivImageNameResolver = network.PixivImageNameResolver;
            //NetworkSettings.PixivImageNameResolver2 = network.PixivImageNameResolver2;

            //NetworkSettings.GitHubNameResolver = network.GitHubNameResolver;
            //NetworkSettings.GitHubApiNameResolver = network.GitHubApiNameResolver;
            //NetworkSettings.GitHubAvatarNameResolver = network.GitHubAvatarNameResolver;
            //NetworkSettings.GitHubUserContentNameResolver = network.GitHubUserContentNameResolver;
            //NetworkSettings.GitHubAssetsNameResolver = network.GitHubAssetsNameResolver;
            //NetworkSettings.GitHubCodeloadNameResolver = network.GitHubCodeloadNameResolver;
        }
    }

    [SettingsEntry(Symbol.Apps, SettingsGroupApplicationHeader, null)]
    public ApplicationSettingsGroup ApplicationSettings { get; set; } = new();

    [SettingsEntry(Symbol.WiFi, SettingsGroupNetworkHeader, null)]
    public NetworkSettingsGroup NetworkSettings { get; set; } = new();

    [SettingsEntry(Symbol.News, SettingsGroupBrowsingExperienceHeader, null)]
    public BrowsingExperienceSettingsGroup BrowsingExperienceSettings { get; set; } = new();

    [SettingsEntry(Symbol.SearchSparkle, SettingsGroupSearchHeader, null)]
    public SearchSettingsGroup SearchSettings { get; set; } = new();

    [SettingsEntry(Symbol.ArrowSquareDown, SettingsGroupDownloadHeader, null)]
    public DownloadSettingsGroup DownloadSettings { get; set; } = new();

#if PIXEVAL_MCP
    [SettingsEntry(Symbol.Bot, SettingsGroupMcpHeader, null)]
    public McpSettingsGroup McpSettings { get; set; } = new();
#endif

    [SettingsEntry(Symbol.Settings, SettingsGroupNovelHeader, null)]
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
            NetworkSettings.EnablePixivDomainFronting,
            NetworkSettings.PixivDomainFrontingType,
            MakoHelper.ToMakoProxy(NetworkSettings.ProxyType, NetworkSettings.Proxy),
            NetworkSettings.WebCookie,
            NetworkSettings.MirrorHost,
            BrowsingExperienceSettings.TargetFilter,
            700,
            CultureInfo.CurrentCulture);
    }
}

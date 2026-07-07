// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Mcp.Dtos;

namespace Pixeval.Utilities.McpServer;

public sealed partial class PixevalMcpService
{
    public PixevalSettingsSummaryDto SettingsSummary()
    {
        var settings = ViewModel.AppSettings;
        var application = settings.ApplicationSettings;
        var network = settings.NetworkSettings;
        var browsing = settings.BrowsingExperienceSettings;
        var search = settings.SearchSettings;
        var download = settings.DownloadSettings;
        var mcp = settings.McpSettings;
        var novel = settings.NovelSettings;

        return new(
            new(
                application.CultureName,
                application.Theme.ToString(),
                settings.ActualTheme.ToString(),
                application.UseFileCache,
                application.LimitFileCacheSize,
                application.FileCacheSizeLimitInMegabytes,
                application.AppFontFamily.Count),
            new(
                network.EnablePixivDomainFronting,
                network.PixivDomainFrontingType.ToString(),
                network.ProxyType.ToString(),
                !string.IsNullOrWhiteSpace(network.Proxy),
                network.EnableGitHubDomainFronting,
                !string.IsNullOrWhiteSpace(network.MirrorHost),
                !string.IsNullOrWhiteSpace(network.WebCookie),
                new(
                    network.PixivAppApiNameResolver.Count,
                    network.PixivWebApiNameResolver.Count,
                    network.PixivAccountNameResolver.Count,
                    network.PixivOAuthNameResolver.Count,
                    network.PixivImageNameResolver.Count,
                    network.PixivImageNameResolver2.Count),
                new(
                    network.GitHubNameResolver.Count,
                    network.GitHubApiNameResolver.Count,
                    network.GitHubAvatarNameResolver.Count,
                    network.GitHubUserContentNameResolver.Count,
                    network.GitHubAssetsNameResolver.Count,
                    network.GitHubCodeloadNameResolver.Count)),
            new(
                browsing.ThumbnailLayoutType.ToString(),
                browsing.BrowseMode.ToString(),
                browsing.BrowseDirection.ToString(),
                browsing.IllustrationViewerAutoPlayInterval,
                browsing.IllustrationViewerAutoPlayMode.ToString(),
                browsing.IllustrationViewerAutoPlayScope.ToString(),
                browsing.TargetFilter.ToString(),
                browsing.BlockedTags.Count,
                browsing.OpenWorkInfoByDefault,
                browsing.OpenUserInfoByDefault),
            new(
                !string.IsNullOrWhiteSpace(search.SauceNaoApiKey),
                search.DefaultSimpleWorkType.ToString(),
                search.IllustrationRankOption.ToString(),
                search.NovelRankOption.ToString()),
            new(
                download.OverwriteDownloadedFile,
                download.MaxDownloadTaskConcurrencyLevel,
                download.DownloadPathMacro,
                download.IllustrationDownloadFormat,
                download.UgoiraDownloadFormat,
                download.NovelDownloadFormat),
            new(
                mcp.EnableServer,
                Port,
                mcp.EnableWriteTools,
                true,
                MaxBinaryResourceMegabytes),
            new(
                novel.NovelFontWeight.ToString(),
                novel.NovelFontFamily.Count,
                novel.NovelFontSize,
                novel.NovelLineHeight,
                novel.NovelMaxWidth));
    }
}

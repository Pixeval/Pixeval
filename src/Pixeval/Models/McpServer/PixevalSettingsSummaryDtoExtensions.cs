// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.AppManagement;
using Pixeval.Mcp.Dtos;

namespace Pixeval.Models.McpServer;

internal static class PixevalSettingsSummaryDtoExtensions
{
    extension(AppSettings settings)
    {
        public PixevalSettingsSummaryDto ToMcpDto() =>
            new(
                settings.ApplicationSettings.ToMcpDto(settings.ActualTheme.ToString()),
                settings.NetworkSettings.ToMcpDto(),
                settings.BrowsingExperienceSettings.ToMcpDto(),
                settings.SearchSettings.ToMcpDto(),
                settings.DownloadSettings.ToMcpDto(),
                settings.McpSettings.ToMcpDto(),
                settings.NovelSettings.ToMcpDto());
    }

    extension(ApplicationSettingsGroup settings)
    {
        public PixevalApplicationSettingsSummaryDto ToMcpDto(string actualTheme) =>
            new(
                settings.CultureName,
                settings.Theme.ToString(),
                actualTheme,
                settings.UseFileCache,
                settings.LimitFileCacheSize,
                settings.FileCacheSizeLimitInMegabytes,
                settings.AppFontFamily.Count);
    }

    extension(NetworkSettingsGroup settings)
    {
        public PixevalNetworkSettingsSummaryDto ToMcpDto() =>
            new(
                settings.EnablePixivDomainFronting,
                settings.PixivDomainFrontingType.ToString(),
                settings.ProxyType.ToString(),
                !string.IsNullOrWhiteSpace(settings.Proxy),
                settings.EnableGitHubDomainFronting,
                !string.IsNullOrWhiteSpace(settings.MirrorHost),
                !string.IsNullOrWhiteSpace(settings.WebCookie),
                ToPixivNameResolverMcpDto(settings),
                ToGitHubNameResolverMcpDto(settings));
    }

    extension(BrowsingExperienceSettingsGroup settings)
    {
        public PixevalBrowsingExperienceSettingsSummaryDto ToMcpDto() =>
            new(
                settings.ThumbnailLayoutType.ToString(),
                settings.BrowseMode.ToString(),
                settings.BrowseDirection.ToString(),
                settings.IllustrationViewerAutoPlayInterval,
                settings.IllustrationViewerAutoPlayMode.ToString(),
                settings.IllustrationViewerAutoPlayScope.ToString(),
                settings.TargetFilter.ToString(),
                settings.BlockedTags.Count,
                settings.OpenWorkInfoByDefault,
                settings.OpenUserInfoByDefault);
    }

    extension(SearchSettingsGroup settings)
    {
        public PixevalSearchSettingsSummaryDto ToMcpDto() =>
            new(
                !string.IsNullOrWhiteSpace(settings.SauceNaoApiKey),
                settings.DefaultSimpleWorkType.ToString(),
                settings.IllustrationRankOption.ToString(),
                settings.NovelRankOption.ToString());
    }

    extension(DownloadSettingsGroup settings)
    {
        public PixevalDownloadSettingsSummaryDto ToMcpDto() =>
            new(
                settings.OverwriteDownloadedFile,
                settings.MaxDownloadTaskConcurrencyLevel,
                settings.DownloadPathMacro,
                settings.IllustrationDownloadFormat,
                settings.UgoiraDownloadFormat,
                settings.NovelDownloadFormat);
    }

    extension(McpSettingsGroup settings)
    {
        public PixevalMcpSettingsSummaryDto ToMcpDto() =>
            new(
                settings.EnableServer,
                settings.Port,
                settings.EnableWriteTools,
                settings.MaxBinaryResourceMegabytes);
    }

    extension(NovelSettingsGroup settings)
    {
        public PixevalNovelSettingsSummaryDto ToMcpDto() =>
            new(
                settings.NovelFontWeight.ToString(),
                settings.NovelFontFamily.Count,
                settings.NovelFontSize,
                settings.NovelLineHeight,
                settings.NovelMaxWidth);
    }

    private static PixevalPixivNameResolverSummaryDto ToPixivNameResolverMcpDto(NetworkSettingsGroup settings) =>
        new(
            settings.PixivAppApiNameResolver.Count,
            settings.PixivWebApiNameResolver.Count,
            settings.PixivAccountNameResolver.Count,
            settings.PixivOAuthNameResolver.Count,
            settings.PixivImageNameResolver.Count,
            settings.PixivImageNameResolver2.Count);

    private static PixevalGitHubNameResolverSummaryDto ToGitHubNameResolverMcpDto(NetworkSettingsGroup settings) =>
        new(
            settings.GitHubNameResolver.Count,
            settings.GitHubApiNameResolver.Count,
            settings.GitHubAvatarNameResolver.Count,
            settings.GitHubUserContentNameResolver.Count,
            settings.GitHubAssetsNameResolver.Count,
            settings.GitHubCodeloadNameResolver.Count);
}

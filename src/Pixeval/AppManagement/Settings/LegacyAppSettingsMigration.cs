// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.IO;
using SharpYaml;
using SharpYaml.Model;

namespace Pixeval.AppManagement.Settings;

/// <summary>
/// Temporary migration for settings written before the nested expander settings were introduced.
/// Remove this reader once support for those configuration files is no longer needed.
/// </summary>
public static class LegacyAppSettingsMigration
{
    public static AppSettings? Deserialize(Stream stream)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var yaml = YamlStream.Load(reader, new YamlNodeTracker());
        if (yaml.Count is 1 && yaml[0].Contents is YamlMapping root)
        {
            MoveProperties(root, nameof(AppSettings.ApplicationSettings), nameof(ApplicationSettingsGroup.HomePage),
                [
                    nameof(HomePageSettings.HomePageRows),
                    nameof(HomePageSettings.HomePageColumns),
                    nameof(HomePageSettings.HideHomePageToolbar),
                    nameof(HomePageSettings.HideHomePageCardTitle)
                ]);
            MoveProperties(root, nameof(AppSettings.BrowsingExperienceSettings), nameof(BrowsingExperienceSettingsGroup.AutoPlay),
                [
                    nameof(AutoPlaySettings.IllustrationViewerAutoPlayInterval),
                    nameof(AutoPlaySettings.IllustrationViewerAutoPlayMode),
                    nameof(AutoPlaySettings.IllustrationViewerAutoPlayScope)
                ]);
            MoveProperties(root, nameof(AppSettings.ApplicationSettings), nameof(ApplicationSettingsGroup.FileCache),
                [
                    nameof(FileCacheSettings.LimitFileCacheSize),
                    nameof(FileCacheSettings.FileCacheSizeLimitInMegabytes)
                ]);
            MoveProperties(root, nameof(AppSettings.BrowsingExperienceSettings), nameof(BrowsingExperienceSettingsGroup.ThumbnailLayout),
                [
                    nameof(ThumbnailLayoutSettings.ThumbnailLayoutType),
                    nameof(ThumbnailLayoutSettings.IllustrationLinedFlowItemHeight),
                    nameof(ThumbnailLayoutSettings.IllustrationGridItemSize),
                    nameof(ThumbnailLayoutSettings.IllustrationGridLineSize),
                    nameof(ThumbnailLayoutSettings.IllustrationMasonryColumnWidth)
                ]);
            MoveProperties(root, nameof(AppSettings.SearchSettings), nameof(SearchSettingsGroup.RankOptions),
                [
                    nameof(RankOptionsSettings.IllustrationRankOption),
                    nameof(RankOptionsSettings.NovelRankOption)
                ]);
            MoveProperties(root, nameof(AppSettings.DownloadSettings), nameof(DownloadSettingsGroup.DownloadFormats),
                [
                    nameof(DownloadFormatsSettings.IllustrationDownloadFormat),
                    nameof(DownloadFormatsSettings.UgoiraDownloadFormat),
                    nameof(DownloadFormatsSettings.NovelDownloadFormat)
                ]);
            MoveProperties(root, nameof(AppSettings.NetworkSettings), nameof(NetworkSettingsGroup.PixivDomainFronting),
                [
                    nameof(PixivDomainFrontingSettings.EnablePixivDomainFronting),
                    nameof(PixivDomainFrontingSettings.PixivDomainFrontingType),
                    nameof(PixivDomainFrontingSettings.PixivAppApiNameResolver),
                    nameof(PixivDomainFrontingSettings.PixivWebApiNameResolver),
                    nameof(PixivDomainFrontingSettings.PixivAccountNameResolver),
                    nameof(PixivDomainFrontingSettings.PixivOAuthNameResolver),
                    nameof(PixivDomainFrontingSettings.PixivImageNameResolver),
                    nameof(PixivDomainFrontingSettings.PixivImageNameResolver2)
                ]);
            MoveProperties(root, nameof(AppSettings.NetworkSettings), nameof(NetworkSettingsGroup.GitHubDomainFronting),
                [
                    nameof(GitHubDomainFrontingSettings.EnableGitHubDomainFronting),
                    nameof(GitHubDomainFrontingSettings.GitHubNameResolver),
                    nameof(GitHubDomainFrontingSettings.GitHubApiNameResolver),
                    nameof(GitHubDomainFrontingSettings.GitHubAvatarNameResolver),
                    nameof(GitHubDomainFrontingSettings.GitHubUserContentNameResolver),
                    nameof(GitHubDomainFrontingSettings.GitHubAssetsNameResolver),
                    nameof(GitHubDomainFrontingSettings.GitHubCodeloadNameResolver)
                ]);
            MoveProperties(root, nameof(AppSettings.NetworkSettings), nameof(NetworkSettingsGroup.ProxySettings),
                [
                    nameof(ProxySettings.ProxyType),
                    nameof(ProxySettings.Proxy)
                ]);
        }

        // Keep type conversion and validation in the AOT-compatible generated serializer.
        return YamlSerializer.Deserialize(yaml.ToString(), SettingsSerializerContext.Default.AppSettings);
    }

    private static void MoveProperties(YamlMapping root, string groupName, string nestedName, ReadOnlySpan<string> propertyNames)
    {
        if (!root.TryGetValue(groupName, out var groupNode) || groupNode is not YamlMapping group)
            return;

        var hasNested = group.TryGetValue(nestedName, out var nestedNode);
        var nested = nestedNode as YamlMapping;
        foreach (var name in propertyNames)
        {
            if (!group.TryGetValue(name, out var value))
                continue;

            if (!hasNested)
            {
                nested = new YamlMapping();
                group[nestedName] = nested;
                hasNested = true;
            }

            // Explicit new values win, including false, zero and empty collections.
            // Leave an invalid new node untouched so deserialization can report the error.
            if (nested is not null && !nested.ContainsKey(name))
                nested[name] = value;
            _ = group.Remove(name);
        }
    }
}

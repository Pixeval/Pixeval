using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.AppManagement;
using Pixeval.AppManagement.Settings;
using Pixeval.Models.Options;
using SharpYaml;
using SharpYaml.Model;

namespace Pixeval.Tests;

[TestClass]
public sealed class LegacyAppSettingsMigrationTest
{
    [TestMethod]
    public void OldSettingsShouldMigrateAllExpanderGroupsAndSaveOnlyNestedFields()
    {
        var settings = Read(
            """
            ApplicationSettings:
              LimitFileCacheSize: true
              FileCacheSizeLimitInMegabytes: 123
              HomePageRows: 9
            BrowsingExperienceSettings:
              ThumbnailLayoutType: Grid
              IllustrationLinedFlowItemHeight: 111
              IllustrationGridItemSize: 222
              IllustrationGridLineSize: 333
              IllustrationMasonryColumnWidth: 444
            SearchSettings:
              IllustrationRankOption: Month
              NovelRankOption: Week
            DownloadSettings:
              IllustrationDownloadFormat: custom-image
              UgoiraDownloadFormat: custom-animation
              NovelDownloadFormat: custom-novel
            NetworkSettings:
              EnablePixivDomainFronting: false
              PixivDomainFrontingType: Fragmentation
              PixivAppApiNameResolver: [127.0.0.1]
              PixivWebApiNameResolver: [127.0.0.2]
              PixivAccountNameResolver: [127.0.0.3]
              PixivOAuthNameResolver: [127.0.0.4]
              PixivImageNameResolver: [127.0.0.5]
              PixivImageNameResolver2: [127.0.0.6]
              EnableGitHubDomainFronting: false
              GitHubNameResolver: [127.0.0.7]
              GitHubApiNameResolver: [127.0.0.8]
              GitHubAvatarNameResolver: [127.0.0.9]
              GitHubUserContentNameResolver: [127.0.0.10]
              GitHubAssetsNameResolver: [127.0.0.11]
              GitHubCodeloadNameResolver: []
              ProxyType: Custom
              Proxy: http://localhost:4321
            """);

        Assert.IsTrue(settings.ApplicationSettings.FileCache.LimitFileCacheSize);
        Assert.AreEqual(123, settings.ApplicationSettings.FileCache.FileCacheSizeLimitInMegabytes);
        Assert.AreEqual(9, settings.ApplicationSettings.HomePageRows);
        Assert.AreEqual(ThumbnailLayoutType.Grid, settings.BrowsingExperienceSettings.ThumbnailLayout.ThumbnailLayoutType);
        Assert.AreEqual(111, settings.BrowsingExperienceSettings.ThumbnailLayout.IllustrationLinedFlowItemHeight);
        Assert.AreEqual(222, settings.BrowsingExperienceSettings.ThumbnailLayout.IllustrationGridItemSize);
        Assert.AreEqual(333, settings.BrowsingExperienceSettings.ThumbnailLayout.IllustrationGridLineSize);
        Assert.AreEqual(444, settings.BrowsingExperienceSettings.ThumbnailLayout.IllustrationMasonryColumnWidth);
        Assert.AreEqual(Mako.Global.Enum.RankOption.Month, settings.SearchSettings.RankOptions.IllustrationRankOption);
        Assert.AreEqual(Mako.Global.Enum.RankOption.Week, settings.SearchSettings.RankOptions.NovelRankOption);
        Assert.AreEqual("custom-image", settings.DownloadSettings.DownloadFormats.IllustrationDownloadFormat);
        Assert.AreEqual("custom-animation", settings.DownloadSettings.DownloadFormats.UgoiraDownloadFormat);
        Assert.AreEqual("custom-novel", settings.DownloadSettings.DownloadFormats.NovelDownloadFormat);
        var pixiv = settings.NetworkSettings.PixivDomainFronting;
        Assert.IsFalse(pixiv.EnablePixivDomainFronting);
        Assert.AreEqual("127.0.0.1", pixiv.PixivAppApiNameResolver[0]);
        Assert.AreEqual("127.0.0.2", pixiv.PixivWebApiNameResolver[0]);
        Assert.AreEqual("127.0.0.3", pixiv.PixivAccountNameResolver[0]);
        Assert.AreEqual("127.0.0.4", pixiv.PixivOAuthNameResolver[0]);
        Assert.AreEqual("127.0.0.5", pixiv.PixivImageNameResolver[0]);
        Assert.AreEqual("127.0.0.6", pixiv.PixivImageNameResolver2[0]);
        var gitHub = settings.NetworkSettings.GitHubDomainFronting;
        Assert.IsFalse(gitHub.EnableGitHubDomainFronting);
        Assert.AreEqual("127.0.0.7", gitHub.GitHubNameResolver[0]);
        Assert.AreEqual("127.0.0.8", gitHub.GitHubApiNameResolver[0]);
        Assert.AreEqual("127.0.0.9", gitHub.GitHubAvatarNameResolver[0]);
        Assert.AreEqual("127.0.0.10", gitHub.GitHubUserContentNameResolver[0]);
        Assert.AreEqual("127.0.0.11", gitHub.GitHubAssetsNameResolver[0]);
        Assert.HasCount(0, gitHub.GitHubCodeloadNameResolver);
        Assert.AreEqual(ProxyType.Custom, settings.NetworkSettings.ProxySettings.ProxyType);
        Assert.AreEqual("http://localhost:4321", settings.NetworkSettings.ProxySettings.Proxy);

        var saved = YamlSerializer.Serialize(settings, SettingsSerializerContext.Default.AppSettings);
        using var reader = new StringReader(saved);
        var root = (YamlMapping) YamlStream.Load(reader, new YamlNodeTracker())[0].Contents!;
        Assert.IsFalse(((YamlMapping) root["NetworkSettings"]!).ContainsKey("Proxy"));
        Assert.IsFalse(((YamlMapping) root["ApplicationSettings"]!).ContainsKey("LimitFileCacheSize"));
        Assert.AreEqual(saved, YamlSerializer.Serialize(Read(saved), SettingsSerializerContext.Default.AppSettings));
    }

    [TestMethod]
    public void NewValuesShouldWinWhileMissingNestedFieldsAreFilledFromOldValues()
    {
        var settings = Read(
            """
            ApplicationSettings:
              LimitFileCacheSize: true
              FileCacheSizeLimitInMegabytes: 123
              FileCache:
                LimitFileCacheSize: false
            NetworkSettings:
              GitHubNameResolver: [127.0.0.1]
              GitHubDomainFronting:
                GitHubNameResolver: []
              Proxy: old-address
              ProxySettings:
                Proxy: ''
            BrowsingExperienceSettings:
              IllustrationGridItemSize: 123
              ThumbnailLayout:
                IllustrationGridItemSize: 0
            """);

        Assert.IsFalse(settings.ApplicationSettings.FileCache.LimitFileCacheSize);
        Assert.AreEqual(123, settings.ApplicationSettings.FileCache.FileCacheSizeLimitInMegabytes);
        Assert.AreEqual(0, settings.BrowsingExperienceSettings.ThumbnailLayout.IllustrationGridItemSize);
        Assert.HasCount(0, settings.NetworkSettings.GitHubDomainFronting.GitHubNameResolver);
        Assert.AreEqual("", settings.NetworkSettings.ProxySettings.Proxy);
        Assert.AreEqual(new RankOptionsSettings(), settings.SearchSettings.RankOptions);
    }

    private static AppSettings Read(string yaml)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(yaml));
        return LegacyAppSettingsMigration.Deserialize(stream)!;
    }
}

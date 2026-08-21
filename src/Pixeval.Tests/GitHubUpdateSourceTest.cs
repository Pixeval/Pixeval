using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.AppManagement;
using Pixeval.Models.Options;
using Pixeval.Utilities.GitHub;
using Velopack;
using Velopack.Logging;
using Velopack.Locators;
using Velopack.Sources;

namespace Pixeval.Tests;

[TestClass]
public sealed class GitHubUpdateSourceTest
{
    [TestMethod]
    [TestCategory("Integration")]
    public async Task StableWinX64FeedContainsLatestPublishedRelease()
    {
        using var client = CreatePixevalGitHubClient();

        var source = new GithubSource(
            "https://github.com/Pixeval/Pixeval",
            string.Empty,
            prerelease: false,
            downloader: new GitHubFileDownloader(() => client));

        var feed = await source.GetReleaseFeed(
            NullVelopackLogger.Instance,
            appId: null,
            channel: "win-x64");
        var latestFull = feed.Assets
            .Where(static asset => asset.Type is VelopackAssetType.Full)
            .MaxBy(static asset => asset.Version.Version);

        if (latestFull is null)
        {
            Assert.Fail("The GitHub source returned no Windows x64 full release.");
            return;
        }

        Assert.IsGreaterThanOrEqualTo(new Version(5, 0, 11), latestFull.Version.Version);
        Assert.IsTrue(latestFull.FileName.EndsWith("-win-x64-full.nupkg", StringComparison.Ordinal));
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task UpdateManagerReportsNoUpdateForCurrentPublishedRelease()
    {
        using var client = CreatePixevalGitHubClient();

        var source = new GithubSource(
            "https://github.com/Pixeval/Pixeval",
            string.Empty,
            prerelease: false,
            downloader: new GitHubFileDownloader(() => client));
        var locator = new TestVelopackLocator(
            "Pixeval",
            "5.0.11",
            Path.GetTempPath(),
            appDir: null,
            rootDir: null,
            updateExe: null,
            channel: "win-x64",
            logger: NullVelopackLogger.Instance);
        var manager = new UpdateManager(
            source,
            new UpdateOptions { MaximumDeltasBeforeFallback = 10 },
            locator);

        var update = await manager.CheckForUpdatesAsync();

        Assert.IsNull(update);
    }

    private static HttpClient CreatePixevalGitHubClient() =>
        GitHubDirectHttpClientFactory.Create(
            new NetworkSettingsGroup { ProxyType = ProxyType.System },
            TimeSpan.FromSeconds(30));
}

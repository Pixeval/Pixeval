using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using Semver;

namespace Pixeval.AppManagement;

public class Versioning
{
    public SemVersion CurrentVersion { get; } = SemVersion.Parse(ThisAssembly.Git.Tag, SemVersionStyles.Strict);

    public SemVersion? NewestVersion => NewestAppReleaseModel?.Version;

    public AppReleaseModel? NewestAppReleaseModel => AppReleaseModels?[0];

    public AppReleaseModel? CurrentAppReleaseModel => AppReleaseModels?.FirstOrDefault(t => t.Version == CurrentVersion);

    public UpdateState CompareUpdateState(SemVersion currentVersion, SemVersion? newVersion)
    {
        if (newVersion is null)
            return UpdateState.Unknown;

        return currentVersion.ComparePrecedenceTo(newVersion) switch
        {
            > 0 => UpdateState.Insider,
            0 => UpdateState.UpToDate,
            _ => newVersion.Major > currentVersion.Major ? UpdateState.MajorUpdate :
                newVersion.Minor > currentVersion.Minor ? UpdateState.MinorUpdate :
                newVersion.Patch > currentVersion.Patch ? UpdateState.BuildUpdate :
                UpdateState.SpecifierUpdate
        };
    }

    public UpdateState UpdateState { get; private set; }

    public bool UpdateAvailable { get; private set; }

    public AppReleaseModel[]? AppReleaseModels { get; private set; }

    public async Task GitHubCheckForUpdateAsync(HttpClient client)
    {
        try
        {
            AppReleaseModels = null;
            if (client.DefaultRequestHeaders.UserAgent.Count is 0)
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36 Edg/121.0.0.0");
            if (await client.GetFromJsonAsync("https://api.github.com/repos/Pixeval/Pixeval/releases", typeof(GitHubRelease[]), GitHubReleaseSerializeContext.Default) is GitHubRelease[] { Length: > 0 } gitHubReleases)
            {
                var appReleaseModels = new List<AppReleaseModel>(gitHubReleases.Length);
                foreach (var release in gitHubReleases)
                {
                    var tag = release.TagName;
                    if (SemVersion.TryParse(tag, SemVersionStyles.Strict, out var appVersion))
                    {
                        App.AppViewModel.AppSettings.LastCheckedUpdate = DateTimeOffset.Now;
                        var str = release.Assets.FirstOrDefault(t =>
                            t.BrowserDownloadUrl.EndsWith(RuntimeInformation.ProcessArchitecture + ".exe",
                                StringComparison.OrdinalIgnoreCase))?.BrowserDownloadUrl;
                        var uri = str is null ? null : new Uri(str);

                        appReleaseModels.Add(new AppReleaseModel(
                            appVersion,
                            release.Notes,
                            uri));
                    }
                }

                appReleaseModels.Sort();
                appReleaseModels.Reverse();

                AppReleaseModels = [.. appReleaseModels];
            }
        }
        catch
        {
            // ignored
        }
        UpdateState = AppReleaseModels is null ? UpdateState.Unknown : CompareUpdateState(CurrentVersion, NewestVersion);
        UpdateAvailable = UpdateState is not UpdateState.UpToDate and not UpdateState.Insider and not UpdateState.Unknown;
    }
}

public record AppReleaseModel(
    SemVersion Version,
    string ReleaseNote,
    Uri? ReleaseUri) : IComparable<AppReleaseModel>
{
    public int CompareTo(AppReleaseModel? other)
    {
        if (ReferenceEquals(this, other))
            return 0;
        if (other is null)
            return 1;
        return Version.ComparePrecedenceTo(other.Version);
    }
}

[JsonSerializable(typeof(GitHubRelease[]))]
[JsonSerializable(typeof(Assets[]))]
public partial class GitHubReleaseSerializeContext : JsonSerializerContext;

public class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public required string TagName { get; init; }

    [JsonPropertyName("assets")]
    public required Assets[] Assets { get; init; }

    [JsonPropertyName("body")]
    public required string Notes { get; init; }
}

public class Assets
{
    [JsonPropertyName("browser_download_url")]
    public required string BrowserDownloadUrl { get; init; }
}

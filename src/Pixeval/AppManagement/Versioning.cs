using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Pixeval.AppManagement;

public class Versioning
{
    public PackageVersion CurrentVersion { get; } = Package.Current.Id.Version;

    public PackageVersion? NewestVersion => NewestAppReleaseModel?.Version;

    public AppReleaseModel? NewestAppReleaseModel => AppReleaseModels?[0];

    public AppReleaseModel? CurrentAppReleaseModel => AppReleaseModels?.FirstOrDefault(t => t.Version == CurrentVersion);

    public UpdateState CompareUpdateState(PackageVersion currentVersion, PackageVersion? newVersion)
    {
        if (newVersion is not { } version)
            return UpdateState.Unknown;

        var currentLong = ((ulong)currentVersion.Major << 0x30) +
                          ((ulong)currentVersion.Minor << 0x20) + 
                          ((ulong)currentVersion.Build << 0x10) + 
                          currentVersion.Revision;
        var newLong = ((ulong)version.Major << 0x30) + 
                      ((ulong)version.Minor << 0x20) + 
                      ((ulong)version.Build << 0x10) + 
                      version.Revision;

        if (currentLong > newLong)
            return UpdateState.Insider;

        return (newLong - currentLong) switch
        {
            0 => UpdateState.UpToDate,
            > 0x30 => UpdateState.MajorUpdate,
            > 0x20 => UpdateState.MinorUpdate,
            > 0x10 => UpdateState.BuildUpdate,
            _ => UpdateState.RevisionUpdate
        };
    }

    public UpdateState UpdateState { get; private set; }

    public bool UpdateAvailable => UpdateState is not UpdateState.UpToDate and not UpdateState.Insider and not UpdateState.Unknown;

    public AppReleaseModel[]? AppReleaseModels { get; private set; }

    public async Task GitHubCheckForUpdateAsync(HttpClient client)
    {
        try
        {
            if (client.DefaultRequestHeaders.UserAgent.Count is 0)
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36 Edg/121.0.0.0");
            if (await client.GetFromJsonAsync("https://api.github.com/repos/Pixeval/Pixeval/releases", typeof(GitHubRelease[]), GitHubReleaseSerializeContext.Default) is GitHubRelease[] { Length: > 0 } gitHubReleases)
            {
                var appReleaseModels = new List<AppReleaseModel>(gitHubReleases.Length);
                foreach (var release in gitHubReleases)
                {
                    var tags = release.TagName.Split('.')
                        .Select(t => ushort.TryParse(t, out var result) ? result : (ushort)0u)
                        .Concat(Enumerable.Repeat((ushort)0u, 4)).ToArray();
                    var version = new PackageVersion(tags[0], tags[1], tags[2], tags[3]);
                    var str = release.Assets.FirstOrDefault(t =>
                        t.BrowserDownloadUrl.EndsWith(RuntimeInformation.ProcessArchitecture + ".exe",
                            StringComparison.OrdinalIgnoreCase))?.BrowserDownloadUrl;
                    var uri = str is null ? null : new Uri(str);

                    appReleaseModels.Add(new AppReleaseModel(
                        version,
                        release.Notes,
                        uri));
                }

                App.AppViewModel.AppSettings.LastCheckedUpdate = DateTimeOffset.Now;

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
    }
}

public record AppReleaseModel(
    PackageVersion Version,
    string ReleaseNote,
    Uri? ReleaseUri) : IComparable<AppReleaseModel>
{
    public int CompareTo(AppReleaseModel? other)
    {
        if (ReferenceEquals(this, other))
            return 0;
        if (other is null)
            return 1;
        var currentLong = ((ulong)Version.Major << 0x30) + 
                          ((ulong)Version.Minor << 0x20) +
                          ((ulong)Version.Build << 0x10) + 
                          Version.Revision;
        var newLong = ((ulong)other.Version.Major << 0x30) + 
                      ((ulong)other.Version.Minor << 0x20) +
                      ((ulong)other.Version.Build << 0x10) + 
                      other.Version.Revision;
        if (currentLong > newLong)
            return 1;
        if (currentLong < newLong)
            return -1;
        return 0;
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

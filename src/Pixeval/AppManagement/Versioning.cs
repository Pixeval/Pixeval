// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Pixeval.AppManagement;

public class Versioning
{
    public Versioning()
    {
        var assembly = typeof(Versioning).Assembly;
        CurrentVersion = assembly.GetName().Version ?? new(0, 0, 0, 0);
        CurrentVersionShortText = CurrentVersion.ToString();
        CurrentVersionFullText = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? CurrentVersionShortText;
    }

    public Version CurrentVersion { get; }

    public string CurrentVersionShortText { get; }

    /// <remarks>
    /// <see cref="AssemblyInformationalVersionAttribute"/> 包含 GitSha 信息
    /// </remarks>
    public string CurrentVersionFullText { get; }

    public Version? NewestVersion => NewestAppReleaseModel?.Version;

    public AppReleaseModel? NewestAppReleaseModel => AppReleaseModels?[0];

    public AppReleaseModel? CurrentAppReleaseModel => AppReleaseModels?.FirstOrDefault(t => t.Version == CurrentVersion);

    public UpdateState CompareUpdateState(Version currentVersion, Version? newVersion)
    {
        if (newVersion is null)
            return UpdateState.Unknown;

        var currentLong =
            ((ulong) currentVersion.Major << 0x30) +
            ((ulong) currentVersion.Minor << 0x20) +
            ((ulong) currentVersion.Build << 0x10) +
            (ulong) currentVersion.Revision;
        var newLong =
            ((ulong) newVersion.Major << 0x30) +
            ((ulong) newVersion.Minor << 0x20) +
            ((ulong) newVersion.Build << 0x10) +
            (ulong) newVersion.Revision;

        if (currentLong > newLong)
            return UpdateState.Insider;

        return (newLong - currentLong) switch
        {
            0 => UpdateState.UpToDate,
            >= 1ul << 0x30 => UpdateState.MajorUpdate,
            >= 1ul << 0x20 => UpdateState.MinorUpdate,
            >= 1ul << 0x10 => UpdateState.BuildUpdate,
            _ => UpdateState.RevisionUpdate
        };
    }

    public UpdateState UpdateState { get; private set; }

    public bool UpdateAvailable => UpdateState is not UpdateState.UpToDate and not UpdateState.Insider and not UpdateState.Unknown;

    public IReadOnlyList<AppReleaseModel>? AppReleaseModels { get; private set; }

    private bool IsUpdating { get; set; }

    public async Task GitHubCheckForUpdateAsync()
    {
        if (IsUpdating)
            return;
        try
        {
            IsUpdating = true;
            var client = App.AppViewModel.GetRequiredGitHubHttpClient();

            if (await client.GetFromJsonAsync("https://api.github.com/repos/Pixeval/Pixeval/releases", GitHubReleaseSerializeContext.Default.GitHubReleaseArray) is { Length: > 0 } gitHubReleases)
            {
                var appReleaseModels = new List<AppReleaseModel>(gitHubReleases.Length);
                foreach (var release in gitHubReleases)
                {
                    var tags = release.TagName.Split('.')
                        .Select(t => ushort.TryParse(t, out var result) ? result : (ushort) 0u)
                        .Concat(Enumerable.Repeat((ushort) 0u, 4)).ToArray();
                    var version = new Version(tags[0], tags[1], tags[2], tags[3]);
                    var str = release.Assets.FirstOrDefault(t =>
                        t.BrowserDownloadUrl.EndsWith(RuntimeInformation.ProcessArchitecture + ".exe",
                            StringComparison.OrdinalIgnoreCase))?.BrowserDownloadUrl;
                    var uri = str is null ? null : new Uri(str);

                    appReleaseModels.Add(new AppReleaseModel(
                        version,
                        release.Notes,
                        uri));
                }

                App.AppViewModel.AppSettings.ApplicationSettings.LastCheckedUpdate = DateTime.UtcNow;

                appReleaseModels.Sort();
                appReleaseModels.Reverse();

                AppReleaseModels = appReleaseModels;
            }
        }
        catch
        {
            // ignored
        }
        finally
        {
            IsUpdating = false;
        }

        UpdateState = AppReleaseModels is null ? UpdateState.Unknown : CompareUpdateState(CurrentVersion, NewestVersion);
    }
}

public record AppReleaseModel(
    Version Version,
    string ReleaseNote,
    Uri? ReleaseUri) : IComparable<AppReleaseModel>
{
    public int CompareTo(AppReleaseModel? other)
    {
        if (ReferenceEquals(this, other))
            return 0;
        if (other is null)
            return 1;
        var currentLong =
            ((ulong) Version.Major << 0x30) +
            ((ulong) Version.Minor << 0x20) +
            ((ulong) Version.Build << 0x10) +
            (ulong) Version.Revision;
        var newLong =
            ((ulong) other.Version.Major << 0x30) +
            ((ulong) other.Version.Minor << 0x20) +
            ((ulong) other.Version.Build << 0x10) +
            (ulong) other.Version.Revision;
        if (currentLong > newLong)
            return 1;
        if (currentLong < newLong)
            return -1;
        return 0;
    }
}

[JsonSerializable(typeof(GitHubRelease[]))]
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

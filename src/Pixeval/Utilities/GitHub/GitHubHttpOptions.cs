// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Pixeval.AppManagement;

namespace Pixeval.Utilities.GitHub;

public static class GitHubHttpOptions
{
    public const string Host = "github.com";

    public const string ApiHost = "api.github.com";

    public const string AvatarHost = "avatars.githubusercontent.com";

    public const string UserContentHost = "*.githubusercontent.com";

    public const string AssetsHost = "github.githubassets.com";

    public const string CodeloadHost = "codeload.github.com";

    public static bool TryGetConfiguredAddresses(
        NetworkSettingsGroup settings,
        string host,
        out IPAddress[] addresses)
    {
        var resolver = GetResolver(settings, host);
        addresses = resolver is null ? [] : [.. resolver.Select(IPAddress.Parse)];
        return addresses.Length is not 0;
    }

    public static bool HasConfiguredResolver(NetworkSettingsGroup settings, string host) =>
        GetResolver(settings, host) is { Count: > 0 };

    private static ObservableCollection<string>? GetResolver(NetworkSettingsGroup settings, string host)
    {
        return host.ToLowerInvariant() switch
        {
            Host or "gist.github.com" => settings.GitHubNameResolver,
            ApiHost => settings.GitHubApiNameResolver,
            AvatarHost => settings.GitHubAvatarNameResolver,
            AssetsHost => settings.GitHubAssetsNameResolver,
            CodeloadHost => settings.GitHubCodeloadNameResolver,
            var h when h.EndsWith(".githubusercontent.com", StringComparison.OrdinalIgnoreCase)
                => settings.GitHubUserContentNameResolver,
            _ => null
        };
    }
}

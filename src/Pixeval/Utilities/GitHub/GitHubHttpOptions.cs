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
        addresses = resolver is null
            ? []
            : [.. resolver.SelectNotNull(static ip => IPAddress.TryParse(ip, out var address) ? address : null)];
        return addresses.Length is not 0;
    }

    public static bool HasConfiguredResolver(NetworkSettingsGroup settings, string host) =>
        GetResolver(settings, host) is { Count: > 0 };

    public static bool IsGitHubHost(string host) =>
        GetResolver(host) is not null;

    private static ObservableCollection<string>? GetResolver(NetworkSettingsGroup settings, string host)
    {
        var resolver = GetResolver(host);
        return resolver switch
        {
            GitHubResolver.Host => settings.GitHubNameResolver,
            GitHubResolver.Api => settings.GitHubApiNameResolver,
            GitHubResolver.Avatar => settings.GitHubAvatarNameResolver,
            GitHubResolver.UserContent => settings.GitHubUserContentNameResolver,
            GitHubResolver.Assets => settings.GitHubAssetsNameResolver,
            GitHubResolver.Codeload => settings.GitHubCodeloadNameResolver,
            _ => null
        };
    }

    private static GitHubResolver? GetResolver(string host)
    {
        return host.ToLowerInvariant() switch
        {
            Host or "gist.github.com" => GitHubResolver.Host,
            ApiHost => GitHubResolver.Api,
            AvatarHost => GitHubResolver.Avatar,
            AssetsHost => GitHubResolver.Assets,
            CodeloadHost => GitHubResolver.Codeload,
            var h when h.EndsWith(".githubusercontent.com", StringComparison.OrdinalIgnoreCase)
                => GitHubResolver.UserContent,
            _ => null
        };
    }

    private enum GitHubResolver
    {
        Host,
        Api,
        Avatar,
        UserContent,
        Assets,
        Codeload
    }
}

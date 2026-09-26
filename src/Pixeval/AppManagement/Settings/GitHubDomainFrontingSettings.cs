// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.ObjectModel;
using AutoSettingsPage;
using FluentIcons.Common;
using Pixeval.Utilities.GitHub;

namespace Pixeval.AppManagement.Settings;

public record GitHubDomainFrontingSettings
{
    public bool EnableGitHubDomainFronting { get; set; } = true;

    [SettingsEntry(Symbol.Box, AppSettingsResources.GitHubNameResolverEntry.Header, AppSettingsResources.GitHubNameResolverEntry.Description, Placeholder = GitHubHttpOptions.Host)]
    public ObservableCollection<string> GitHubNameResolver { get; set; } =
    [
        "20.205.243.166",
        "140.82.112.3",
        "140.82.113.3",
        "140.82.114.3",
        "140.82.121.3"
    ];

    [SettingsEntry(Placeholder = GitHubHttpOptions.ApiHost)]
    public ObservableCollection<string> GitHubApiNameResolver { get; set; } =
    [
        "20.205.243.168",
        "140.82.112.5",
        "140.82.113.5",
        "140.82.114.6",
        "140.82.121.5"
    ];

    [SettingsEntry(Placeholder = GitHubHttpOptions.AvatarHost)]
    public ObservableCollection<string> GitHubAvatarNameResolver { get; set; } =
    [
        "185.199.108.133",
        "185.199.109.133",
        "185.199.110.133",
        "185.199.111.133"
    ];

    [SettingsEntry(Placeholder = GitHubHttpOptions.UserContentHost)]
    public ObservableCollection<string> GitHubUserContentNameResolver { get; set; } =
    [
        "185.199.108.133",
        "185.199.109.133",
        "185.199.110.133",
        "185.199.111.133"
    ];

    [SettingsEntry(Placeholder = GitHubHttpOptions.AssetsHost)]
    public ObservableCollection<string> GitHubAssetsNameResolver { get; set; } =
    [
        "185.199.108.215",
        "185.199.109.215",
        "185.199.110.215",
        "185.199.111.215"
    ];

    [SettingsEntry(Placeholder = GitHubHttpOptions.CodeloadHost)]
    public ObservableCollection<string> GitHubCodeloadNameResolver { get; set; } =
    [
        "20.205.243.165",
        "140.82.112.9",
        "140.82.113.10",
        "140.82.114.10",
        "140.82.121.10"
    ];
}

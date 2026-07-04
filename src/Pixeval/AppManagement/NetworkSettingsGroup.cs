// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.ObjectModel;
using AutoSettingsPage;
using FluentIcons.Common;
using Mako;
using Mako.Net;
using Pixeval.Models.Options;
using Pixeval.Utilities.GitHub;
using static Pixeval.AppSettingsResources;

namespace Pixeval.AppManagement;

public record NetworkSettingsGroup
{
    [SettingsEntry(Symbol.ShieldTask, EnablePixivDomainFrontingEntryHeader, EnablePixivDomainFrontingEntryDescription)]
    public bool EnablePixivDomainFronting { get; set; } = true;

    [SettingsEntry(Symbol.ShieldSettings, PixivDomainFrontingTypeEntryHeader, PixivDomainFrontingTypeEntryDescription)]
    public DomainFrontingType PixivDomainFrontingType { get; set; } = DomainFrontingType.Fragmentation;

    [SettingsEntry(Symbol.Router, ProxyTypeEntryHeader, ProxyTypeEntryDescription)]
    public ProxyType ProxyType { get; set; }

    [SettingsEntry(Symbol.Server, ProxyTextBoxEntryHeader, ProxyTextBoxEntryDescription)]
    public string Proxy { get; set; } = "";

    [SettingsEntry(Symbol.ShieldTask, EnableGitHubDomainFrontingEntryHeader, EnableGitHubDomainFrontingEntryDescription)]
    public bool EnableGitHubDirectConnection { get; set; } = true;

    /// <summary>
    /// The mirror host for image server, Pixeval will do a simple substitution that
    /// changes the host of the original url(i.pximg.net) to this one.
    /// </summary>
    [SettingsEntry(Symbol.HardDrive, ImageMirrorServerEntryHeader, ImageMirrorServerEntryDescription, ImageMirrorServerEntryPlaceholder)]
    public string MirrorHost { get; set; } = "";

    [SettingsEntry(Symbol.Cookies, WebCookieEntryHeader, WebCookieEntryDescription, WebCookieEntryPlaceholder)]
    public string WebCookie { get; set; } = "";

    [SettingsEntry(Symbol.Box, PixivNameResolverEntryHeader, PixivNameResolverEntryDescription, Placeholder = MakoHttpOptions.AppApiHost)]
    public ObservableCollection<string> PixivAppApiNameResolver { get; set; } =
    [
        "104.18.42.239",
        "172.64.145.17"
    ];

    [SettingsEntry(Placeholder = MakoHttpOptions.WebApiHost)]
    public ObservableCollection<string> PixivWebApiNameResolver { get; set; } =
    [
        "210.140.139.155",
        "210.140.139.156",
        "210.140.139.157"
    ];

    [SettingsEntry(Placeholder = MakoHttpOptions.AccountHost)]
    public ObservableCollection<string> PixivAccountNameResolver { get; set; } =
    [
        "210.140.139.155",
        "210.140.139.156",
        "210.140.139.157"
    ];

    [SettingsEntry(Placeholder = MakoHttpOptions.OAuthHost)]
    public ObservableCollection<string> PixivOAuthNameResolver { get; set; } =
    [
        "104.18.42.239",
        "172.64.145.17"
    ];

    [SettingsEntry(Placeholder = MakoHttpOptions.ImageHost)]
    public ObservableCollection<string> PixivImageNameResolver { get; set; } =
    [
        "210.140.139.134",
        "210.140.139.135",
        "210.140.139.136",
        "210.140.139.137"
    ];

    [SettingsEntry(Placeholder = MakoHttpOptions.ImageHost2)]
    public ObservableCollection<string> PixivImageNameResolver2 { get; set; } =
    [
        "210.140.139.135",
        "210.140.139.136",
        "210.140.139.137"
    ];

    [SettingsEntry(Symbol.Box, GitHubNameResolverEntryHeader, GitHubNameResolverEntryDescription, Placeholder = GitHubHttpOptions.Host)]
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
        "185.199.108.154",
        "185.199.109.154",
        "185.199.110.154",
        "185.199.111.154"
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

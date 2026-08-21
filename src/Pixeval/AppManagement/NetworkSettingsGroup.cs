// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.ObjectModel;
using AutoSettingsPage;
using FluentIcons.Common;
using Mako;
using Mako.Net;
using Pixeval.Models.Options;
using Pixeval.Utilities.GitHub;

namespace Pixeval.AppManagement;

public record NetworkSettingsGroup
{
    [SettingsEntry(Symbol.ShieldTask, AppSettingsResources.EnablePixivDomainFrontingEntry.Header, AppSettingsResources.EnablePixivDomainFrontingEntry.Description)]
    public bool EnablePixivDomainFronting { get; set; } = true;

    [SettingsEntry(Symbol.ShieldSettings, AppSettingsResources.PixivDomainFrontingTypeEntry.Header, AppSettingsResources.PixivDomainFrontingTypeEntry.Description)]
    public DomainFrontingType PixivDomainFrontingType { get; set; } = DomainFrontingType.Fragmentation;

    [SettingsEntry(Symbol.Router, AppSettingsResources.ProxyTypeEntry.Header, AppSettingsResources.ProxyTypeEntry.Description)]
    public ProxyType ProxyType { get; set; }

    [SettingsEntry(Symbol.Server, AppSettingsResources.ProxyTextBoxEntry.Header, AppSettingsResources.ProxyTextBoxEntry.Description)]
    public string Proxy { get; set; } = "";

    [SettingsEntry(Symbol.ShieldTask, AppSettingsResources.EnableGitHubDomainFrontingEntry.Header, AppSettingsResources.EnableGitHubDomainFrontingEntry.Description)]
    public bool EnableGitHubDomainFronting { get; set; } = true;

    /// <summary>
    /// The mirror host for image server, Pixeval will do a simple substitution that
    /// changes the host of the original url(i.pximg.net) to this one.
    /// </summary>
    [SettingsEntry(Symbol.HardDrive, AppSettingsResources.ImageMirrorServerEntry.Header, AppSettingsResources.ImageMirrorServerEntry.Description, AppSettingsResources.ImageMirrorServerEntry.Placeholder)]
    public string MirrorHost { get; set; } = "";

    [SettingsEntry(Symbol.Cookies, AppSettingsResources.WebCookieEntry.Header, AppSettingsResources.WebCookieEntry.Description, AppSettingsResources.WebCookieEntry.Placeholder)]
    public string WebCookie { get; set; } = "";

    [SettingsEntry(Symbol.Box, AppSettingsResources.PixivNameResolverEntry.Header, AppSettingsResources.PixivNameResolverEntry.Description, Placeholder = MakoHttpOptions.AppApiHost)]
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

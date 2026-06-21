// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.ObjectModel;
using AutoSettingsPage;
using FluentIcons.Common;
using Mako;
using Mako.Net;
using Pixeval.Models.Options;

namespace Pixeval.AppManagement;

public record NetworkSettingsGroup
{
    [SettingsEntry(Symbol.ShieldTask, AppSettingsResources.EnableDomainFrontingEntryHeader, AppSettingsResources.EnableDomainFrontingEntryDescription)]
    public bool EnableDomainFronting { get; set; } = true;

    [SettingsEntry(Symbol.ShieldSettings, AppSettingsResources.DomainFrontingTypeEntryHeader, AppSettingsResources.DomainFrontingTypeEntryDescription)]
    public DomainFrontingType DomainFrontingType { get; set; } = DomainFrontingType.Fragmentation;

    [SettingsEntry(Symbol.Router, AppSettingsResources.ProxyTypeEntryHeader, AppSettingsResources.ProxyTypeEntryDescription)]
    public ProxyType ProxyType { get; set; }

    [SettingsEntry(Symbol.Server, AppSettingsResources.ProxyTextBoxEntryHeader, AppSettingsResources.ProxyTextBoxEntryDescription)]
    public string Proxy { get; set; } = "";

    /// <summary>
    /// The mirror host for image server, Pixeval will do a simple substitution that
    /// changes the host of the original url(i.pximg.net) to this one.
    /// </summary>
    [SettingsEntry(Symbol.HardDrive, AppSettingsResources.ImageMirrorServerEntryHeader, AppSettingsResources.ImageMirrorServerEntryDescription, AppSettingsResources.ImageMirrorServerEntryPlaceholder)]
    public string MirrorHost { get; set; } = "";

    [SettingsEntry(Symbol.Cookies, AppSettingsResources.WebCookieEntryHeader, AppSettingsResources.WebCookieEntryDescription, AppSettingsResources.WebCookieEntryPlaceholder)]
    public string WebCookie { get; set; } = "";

    [SettingsEntry(Symbol.Box, AppSettingsResources.PixivNameResolverEntryHeader, AppSettingsResources.PixivNameResolverEntryDescription, Placeholder = MakoHttpOptions.AppApiHost)]
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
}

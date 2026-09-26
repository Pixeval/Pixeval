// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using AutoSettingsPage;
using FluentIcons.Common;

namespace Pixeval.AppManagement.Settings;

public record NetworkSettingsGroup
{
    [SettingsEntry(Symbol.Router, AppSettingsResources.ProxyTypeEntry.Header, AppSettingsResources.ProxyTypeEntry.Description)]
    public ProxySettings ProxySettings { get; set; } = new();

    [SettingsEntry(Symbol.ShieldTask, AppSettingsResources.EnableGitHubDomainFrontingEntry.Header, AppSettingsResources.EnableGitHubDomainFrontingEntry.Description)]
    public GitHubDomainFrontingSettings GitHubDomainFronting { get; set; } = new();

    [SettingsEntry(Symbol.ShieldTask, AppSettingsResources.EnablePixivDomainFrontingEntry.Header, AppSettingsResources.EnablePixivDomainFrontingEntry.Description)]
    public PixivDomainFrontingSettings PixivDomainFronting { get; set; } = new();

    /// <summary>
    /// The mirror host for image server, Pixeval will do a simple substitution that
    /// changes the host of the original url(i.pximg.net) to this one.
    /// </summary>
    [SettingsEntry(Symbol.HardDrive, AppSettingsResources.ImageMirrorServerEntry.Header, AppSettingsResources.ImageMirrorServerEntry.Description, AppSettingsResources.ImageMirrorServerEntry.Placeholder)]
    public string MirrorHost { get; set; } = "";

    [SettingsEntry(Symbol.Cookies, AppSettingsResources.WebCookieEntry.Header, AppSettingsResources.WebCookieEntry.Description, AppSettingsResources.WebCookieEntry.Placeholder)]
    public string WebCookie { get; set; } = "";
}

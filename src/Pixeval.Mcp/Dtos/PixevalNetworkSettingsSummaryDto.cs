// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalNetworkSettingsSummaryDto(
    bool EnablePixivDomainFronting,
    string PixivDomainFrontingType,
    string ProxyType,
    bool ProxyConfigured,
    bool EnableGitHubDomainFronting,
    bool MirrorHostConfigured,
    bool WebCookieConfigured,
    PixevalPixivNameResolverSummaryDto PixivNameResolvers,
    PixevalGitHubNameResolverSummaryDto GitHubNameResolvers);
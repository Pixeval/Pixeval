// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalSettingsSummaryDto(
    PixevalApplicationSettingsSummaryDto Application,
    PixevalNetworkSettingsSummaryDto Network,
    PixevalBrowsingExperienceSettingsSummaryDto BrowsingExperience,
    PixevalSearchSettingsSummaryDto Search,
    PixevalDownloadSettingsSummaryDto Download,
    PixevalMcpSettingsSummaryDto Mcp,
    PixevalNovelSettingsSummaryDto Novel);
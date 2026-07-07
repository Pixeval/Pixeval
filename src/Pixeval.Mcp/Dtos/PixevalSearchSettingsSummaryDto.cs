// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalSearchSettingsSummaryDto(
    bool SauceNaoApiKeyConfigured,
    string DefaultSimpleWorkType,
    string IllustrationRankOption,
    string NovelRankOption);
// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalApplicationSettingsSummaryDto(
    string CultureName,
    string Theme,
    string ActualTheme,
    bool UseFileCache,
    bool LimitFileCacheSize,
    int FileCacheSizeLimitInMegabytes,
    int AppFontFamilyCount);
// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalNovelSettingsSummaryDto(
    string NovelFontWeight,
    int NovelFontFamilyCount,
    int NovelFontSize,
    int NovelLineHeight,
    int NovelMaxWidth);
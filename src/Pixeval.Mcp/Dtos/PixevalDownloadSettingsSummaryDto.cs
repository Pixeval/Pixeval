// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalDownloadSettingsSummaryDto(
    bool OverwriteDownloadedFile,
    int MaxDownloadTaskConcurrencyLevel,
    string DownloadPathMacro,
    string IllustrationDownloadFormat,
    string UgoiraDownloadFormat,
    string NovelDownloadFormat);
// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalDownloadMacroAnalysisDto(
    string Text,
    bool IsSuccess,
    IReadOnlyList<PixevalDownloadMacroDiagnosticDto> Diagnostics,
    IReadOnlyList<PixevalDownloadMacroHighlightDto> Highlights);
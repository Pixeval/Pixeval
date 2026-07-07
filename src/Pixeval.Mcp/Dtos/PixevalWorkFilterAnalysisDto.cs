// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalWorkFilterAnalysisDto(
    string Text,
    int CaretPosition,
    bool IsSuccess,
    PixevalWorkFilterQueryDto? Query,
    IReadOnlyList<PixevalWorkFilterDiagnosticDto> Diagnostics,
    IReadOnlyList<PixevalWorkFilterCompletionDto> Completions);
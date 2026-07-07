// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalSetDownloadMacroResultDto(
    bool Success,
    string Message,
    string PreviousMacro,
    string CurrentMacro,
    PixevalDownloadMacroAnalysisDto Analysis);
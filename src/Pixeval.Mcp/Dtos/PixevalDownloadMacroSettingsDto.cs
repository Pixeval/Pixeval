// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalDownloadMacroSettingsDto(
    string CurrentMacro,
    PixevalDownloadMacroAnalysisDto Analysis,
    IReadOnlyList<PixevalDownloadMacroDefinitionDto> AvailableMacros);
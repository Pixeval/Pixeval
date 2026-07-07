// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalDownloadMacroDefinitionDto(
    string Name,
    string Description,
    string Kind,
    string ContextType,
    string? RequiredPredicateName);
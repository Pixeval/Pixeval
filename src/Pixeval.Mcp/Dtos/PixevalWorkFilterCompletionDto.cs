// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalWorkFilterCompletionDto(
    string DisplayText,
    string InsertText,
    int ReplacementStart,
    int ReplacementLength,
    string? Description,
    bool IsHintOnly);
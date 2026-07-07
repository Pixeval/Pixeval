// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalDownloadMacroPreviewItemDto(
    string Label,
    string? WorkType,
    string? WorkId,
    string Path);
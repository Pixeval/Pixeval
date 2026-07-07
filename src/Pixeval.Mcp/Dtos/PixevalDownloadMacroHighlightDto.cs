// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalDownloadMacroHighlightDto(
    string Kind,
    int Start,
    int Length,
    int NestingDepth);
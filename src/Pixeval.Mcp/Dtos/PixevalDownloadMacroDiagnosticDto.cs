// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalDownloadMacroDiagnosticDto(
    string Kind,
    int Start,
    int Length,
    string Message,
    IReadOnlyList<string> Arguments);
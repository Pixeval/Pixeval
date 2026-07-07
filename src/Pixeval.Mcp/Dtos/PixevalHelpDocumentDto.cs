// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalHelpDocumentDto(
    string Topic,
    string Title,
    string ResourceKey,
    string Markdown,
    IReadOnlyList<string> RelatedTools);
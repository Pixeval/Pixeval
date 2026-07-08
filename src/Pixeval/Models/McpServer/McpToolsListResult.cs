using System.Collections.Generic;

namespace Pixeval.Models.McpServer;

internal sealed record McpToolsListResult
{
    public IReadOnlyList<McpToolSchema>? Tools { get; init; }
}

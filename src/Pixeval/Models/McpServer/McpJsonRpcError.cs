namespace Pixeval.Models.McpServer;

internal sealed record McpJsonRpcError
{
    public int? Code { get; init; }

    public string? Message { get; init; }

    public string ToDisplayText()
    {
        var message = string.IsNullOrWhiteSpace(Message) ? "Unknown MCP error." : Message;
        return Code is null ? message : $"{Code}: {message}";
    }
}

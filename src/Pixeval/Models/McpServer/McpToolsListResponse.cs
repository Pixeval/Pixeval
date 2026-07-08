using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Net.ServerSentEvents;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.Models.McpServer;

internal sealed record McpToolsListResponse
{
    public McpJsonRpcError? Error { get; init; }

    public McpToolsListResult? Result { get; init; }

    public static async Task<McpToolsListResponse> ReadFromContentAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        if (content.Headers.ContentType?.MediaType is MediaTypeNames.Text.EventStream)
            return await ReadFromServerSentEventsAsync(stream, cancellationToken);

        return await JsonSerializer.DeserializeAsync(
                   stream,
                   HelpPageJsonContext.Default.McpToolsListResponse,
                   cancellationToken)
               ?? throw new InvalidOperationException("The MCP server response is empty.");
    }

    public IReadOnlyList<McpToolItemViewModel> ToViewModels()
    {
        if (Error is not null)
            throw new InvalidOperationException(Error.ToDisplayText());

        if (Result?.Tools is not { } tools)
            throw new InvalidOperationException("The MCP server response does not contain a tools list.");

        var toolItems = tools
            .Select(static tool => tool.ToViewModel())
            .OfType<McpToolItemViewModel>()
            .OrderBy(static tool => tool.Name, StringComparer.Ordinal)
            .ToArray();
        return toolItems;
    }

    private static async Task<McpToolsListResponse> ReadFromServerSentEventsAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        await foreach (var item in SseParser.Create(stream).EnumerateAsync(cancellationToken))
        {
            var response = JsonSerializer.Deserialize(item.Data, HelpPageJsonContext.Default.McpToolsListResponse);
            if (response is not null)
                return response;
        }

        throw new InvalidOperationException("The MCP server response does not contain a tools list.");
    }
}

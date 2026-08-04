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
        CancellationToken token)
    {
        await using var stream = await content.ReadAsStreamAsync(token);
        if (content.Headers.ContentType?.MediaType is MediaTypeNames.Text.EventStream)
            return await ReadFromServerSentEventsAsync(stream, token);

        return await JsonSerializer.DeserializeAsync(
                   stream,
                   HelpPageJsonContext.Default.McpToolsListResponse,
                   token)
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
        CancellationToken token)
    {
        await foreach (var item in SseParser.Create(stream).EnumerateAsync(token))
        {
            var response = JsonSerializer.Deserialize(item.Data, HelpPageJsonContext.Default.McpToolsListResponse);
            if (response is not null)
                return response;
        }

        throw new InvalidOperationException("The MCP server response does not contain a tools list.");
    }
}

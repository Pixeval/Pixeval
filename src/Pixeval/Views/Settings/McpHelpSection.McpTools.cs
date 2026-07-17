// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.Models.McpServer;

namespace Pixeval.Views.Settings;

public partial class McpHelpSection
{
    private const string ToolsListMethod = "tools/list";

    private const string ToolsListRequestBody = """{"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}""";

    private static readonly HttpClient _McpHttpClient = new() { Timeout = TimeSpan.FromSeconds(8) };

    private static async Task<IReadOnlyList<McpToolItemViewModel>> RequestMcpToolsAsync(
        Uri endpoint,
        CancellationToken cancellationToken)
    {
        var response = await SendToolsListRequestAsync(endpoint, cancellationToken);
        return response.ToViewModels();
    }

    private static async Task<McpToolsListResponse> SendToolsListRequestAsync(
        Uri endpoint,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        request.Headers.TryAddWithoutValidation("Mcp-Method", ToolsListMethod);

        request.Content = new StringContent(ToolsListRequestBody, Encoding.UTF8, "application/json");
        using var response = await _McpHttpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
            return await McpToolsListResponse.ReadFromContentAsync(response.Content, cancellationToken);

        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(
            $"{(int) response.StatusCode} {response.ReasonPhrase}: {TrimErrorMessage(responseText)}");
    }

    private static string TrimErrorMessage(string message)
    {
        const int maxLength = 300;
        var trimmed = message.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength] + "...";
    }
}

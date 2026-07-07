// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Pixeval.Mcp.Dtos;

namespace Pixeval.Mcp;

[McpServerResourceType]
internal sealed class PixevalMcpResources(IPixevalMcpRuntime runtime)
{
    private const int BytesPerMegabyte = 1024 * 1024;

    [McpServerResource(UriTemplate = "pixeval://me", Name = "me", Title = "Current Pixeval account",
        MimeType = "application/json")]
    public string Me() =>
        PixevalMcpResult.Json(PixevalStatusDto.FromRuntime(runtime));

    [McpServerResource(UriTemplate = "pixeval://illust/{id}", Name = "illustration",
        Title = "Pixiv illustration", MimeType = "application/json")]
    public async Task<string> IllustrationAsync(long id, CancellationToken cancellationToken = default)
    {
        runtime.EnsureLoggedIn();
        var illustration = await runtime.MakoClient.GetIllustrationFromIdAsync(id).WaitAsync(cancellationToken)
            .ConfigureAwait(false);
        return PixevalMcpResult.Json(PixevalWorkDto.FromIllustration(illustration));
    }

    [McpServerResource(UriTemplate = "pixeval://illust/{id}/thumbnail/{size}",
        Name = "illustration_thumbnail", Title = "Pixiv illustration thumbnail",
        MimeType = "application/octet-stream")]
    public async Task<BlobResourceContents> IllustrationThumbnailAsync(
        long id,
        string size,
        CancellationToken cancellationToken = default)
    {
        try
        {
            runtime.EnsureLoggedIn();
            var illustration = await runtime.MakoClient.GetIllustrationFromIdAsync(id).WaitAsync(cancellationToken)
                .ConfigureAwait(false);
            return await ImageResourceAsync(
                    PixevalThumbnailInfoDto.GetThumbnailUrl(illustration, size),
                    PixevalThumbnailInfoDto.GetThumbnailResourceUri("illust", id, size),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception e) when (e is not PixevalMcpException and not OperationCanceledException)
        {
            runtime.LogToolException(nameof(IllustrationThumbnailAsync), e);
            throw new PixevalMcpException("Pixeval MCP resource failed. See Pixeval MCP logs for details.");
        }
    }

    [McpServerResource(UriTemplate = "pixeval://novel/{id}/thumbnail/{size}", Name = "novel_thumbnail",
        Title = "Pixiv novel thumbnail", MimeType = "application/octet-stream")]
    public async Task<BlobResourceContents> NovelThumbnailAsync(
        long id,
        string size,
        CancellationToken cancellationToken = default)
    {
        try
        {
            runtime.EnsureLoggedIn();
            var novel = await runtime.MakoClient.GetNovelFromIdAsync(id).WaitAsync(cancellationToken)
                .ConfigureAwait(false);
            return await ImageResourceAsync(
                    PixevalThumbnailInfoDto.GetThumbnailUrl(novel, size),
                    PixevalThumbnailInfoDto.GetThumbnailResourceUri("novel", id, size),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception e) when (e is not PixevalMcpException and not OperationCanceledException)
        {
            runtime.LogToolException(nameof(NovelThumbnailAsync), e);
            throw new PixevalMcpException("Pixeval MCP resource failed. See Pixeval MCP logs for details.");
        }
    }

    private async Task<BlobResourceContents> ImageResourceAsync(
        string url,
        string resourceUri,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new PixevalMcpException("Pixiv did not return an image URL for this thumbnail.");

        var maxBinaryResourceMegabytes = runtime.MaxBinaryResourceMegabytes;
        var maxBinaryResourceBytes = (long) maxBinaryResourceMegabytes * BytesPerMegabyte;
        using var response = await runtime.ImageHttpClient.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken)
            .ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        if (response.Content.Headers.ContentLength is { } length && length > maxBinaryResourceBytes)
            throw new PixevalMcpException(CreateResourceLimitMessage(length, maxBinaryResourceMegabytes));

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        if (bytes.Length > maxBinaryResourceBytes)
            throw new PixevalMcpException(CreateResourceLimitMessage(bytes.Length, maxBinaryResourceMegabytes));

        var mimeType = response.Content.Headers.ContentType?.MediaType ?? PixevalMcpHelpers.GetImageMimeType(url);
        return BlobResourceContents.FromBytes(bytes, resourceUri, mimeType);
    }

    private static string CreateResourceLimitMessage(long bytes, int maxMegabytes) =>
        $"Binary resource is {(double) bytes / BytesPerMegabyte:F2} MB, "
        + $"which exceeds the Pixeval MCP server binary resource limit of {maxMegabytes} MB.";

    [McpServerResource(UriTemplate = "pixeval://novel/{id}", Name = "novel", Title = "Pixiv novel",
        MimeType = "application/json")]
    public async Task<string> NovelAsync(long id, CancellationToken cancellationToken = default)
    {
        runtime.EnsureLoggedIn();
        var novel = await runtime.MakoClient.GetNovelFromIdAsync(id).WaitAsync(cancellationToken).ConfigureAwait(false);
        return PixevalMcpResult.Json(PixevalWorkDto.FromNovel(novel));
    }

    [McpServerResource(UriTemplate = "pixeval://user/{id}", Name = "user", Title = "Pixiv user",
        MimeType = "application/json")]
    public async Task<string> UserAsync(long id, CancellationToken cancellationToken = default)
    {
        runtime.EnsureLoggedIn();
        var user = await runtime.MakoClient.GetUserFromIdAsync(id).WaitAsync(cancellationToken).ConfigureAwait(false);
        return PixevalMcpResult.Json(PixevalUserDto.FromSingleUserResponse(user));
    }
}

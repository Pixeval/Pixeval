// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Mako.Model;
using Pixeval.Mcp;
using Pixeval.Mcp.Dtos;
using Pixeval.Models.SauceNao;

namespace Pixeval.Models.McpServer;

public sealed partial class PixevalMcpService
{
    public async Task<PixevalSauceNaoSearchDto> SauceNaoSearchAsync(
        string? imageBase64,
        string? imageUrl,
        long? illustrationId,
        int page,
        int count,
        string? index,
        double minSimilarity,
        bool loadPixivWorks,
        CancellationToken cancellationToken)
    {
        var apiKey = ViewModel.AppSettings.SearchSettings.SauceNaoApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new PixevalMcpException("Pixeval SauceNAO API key is empty. Configure it in Pixeval settings first.");

        var (bytes, source) = await GetSauceNaoImageBytesAsync(
                imageBase64,
                imageUrl,
                illustrationId,
                page,
                cancellationToken)
            .ConfigureAwait(false);

        using var form = new MultipartFormDataContent();
        using var fileContent = new ReadOnlyMemoryContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "file", "img");

        var request = new SauceNaoRequest(apiKey)
        {
            Index = ParseSauceNaoIndex(index),
            NumberResult = int.Clamp(count, 1, 100)
        };
        var response = await ViewModel.GetRequiredHttpClient()
            .PostAsync(request.ToQueryString(), form, cancellationToken)
            .ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var result = await response.Content
                         .ReadFromJsonAsync(SauceNaoResponseSerializerContext.Default.SauceNaoResponse,
                             cancellationToken)
                         .ConfigureAwait(false)
                     ?? throw new PixevalMcpException("SauceNAO returned an empty response.");

        var filtered = result.Results
            .Where(item => item.Header.Similarity >= minSimilarity)
            .Take(int.Clamp(count, 1, 100))
            .Select(item => ToSauceNaoResultDtoAsync(item, loadPixivWorks, cancellationToken))
            .ToArray();
        var results = await Task.WhenAll(filtered).ConfigureAwait(false);
        return new(result.Header.Status, result.Header.Message, source, results.Length, results);
    }

    private async Task<(byte[] Bytes, string Source)> GetSauceNaoImageBytesAsync(
        string? imageBase64,
        string? imageUrl,
        long? illustrationId,
        int page,
        CancellationToken cancellationToken)
    {
        var providedCount =
            (!string.IsNullOrWhiteSpace(imageBase64) ? 1 : 0)
            + (!string.IsNullOrWhiteSpace(imageUrl) ? 1 : 0)
            + (illustrationId is not null ? 1 : 0);
        if (providedCount is not 1)
            throw new PixevalMcpException("Provide exactly one of imageBase64, imageUrl, or illustrationId.");

        if (!string.IsNullOrWhiteSpace(imageBase64))
        {
            var text = imageBase64.Trim();
            var commaIndex = text.IndexOf(',', StringComparison.Ordinal);
            if (text.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && commaIndex >= 0)
                text = text[(commaIndex + 1)..];

            return (Convert.FromBase64String(text), "base64");
        }

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri)
                || uri.Scheme is not ("http" or "https"))
                throw new PixevalMcpException("imageUrl must be an absolute http or https URL.");

            return (await ViewModel.GetRequiredHttpClient().GetByteArrayAsync(uri, cancellationToken)
                    .ConfigureAwait(false),
                uri.ToString());
        }

        EnsureLoggedIn();
        var illustration = await GetIllustrationAsync(illustrationId!.Value, cancellationToken).ConfigureAwait(false);
        var pages = GetOriginalImagePages(illustration);
        if (page < 0 || page >= pages.Count)
            throw new PixevalMcpException($"page must be between 0 and {pages.Count - 1}.");

        return (await ImageHttpClient.GetByteArrayAsync(pages[page], cancellationToken)
                .ConfigureAwait(false),
            $"pixiv_illustration:{illustrationId}:{page}");
    }

    private static IReadOnlyList<string> GetOriginalImagePages(Illustration illustration) =>
        illustration.PageCount <= 1
            ? [illustration.OriginalSingleUrl ?? illustration.MetaSinglePage.OriginalImageUrl ?? ""]
            : [.. illustration.MetaPages.Select(static page => page.OriginalUrl)];

    private async Task<PixevalSauceNaoResultDto> ToSauceNaoResultDtoAsync(
        SauceNaoResult result,
        bool loadPixivWork,
        CancellationToken cancellationToken)
    {
        PixevalWorkDto? work = null;
        if (loadPixivWork && result.Data.PixivId is { } pixivId)
        {
            EnsureLoggedIn();
            try
            {
                var illustration = await GetIllustrationAsync(pixivId, cancellationToken).ConfigureAwait(false);
                work = PixevalWorkDto.FromIllustration(illustration);
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to load SauceNAO Pixiv hit {pixivId}", e);
            }
        }

        return new(
            result.Header.Similarity,
            result.Header.IndexId.ToString(),
            result.Data.PixivId,
            result.Data.DanbooruId,
            result.Data.YandereId,
            result.Data.GelbooruId,
            result.Data.SankakuId,
            GetSauceNaoWebsiteUrl(result.Data),
            result.Data.PixivId is { } id ? $"pixeval://illust/{id}" : null,
            work);
    }

    private static string? GetSauceNaoWebsiteUrl(SauceNaoData data) =>
        data switch
        {
            { PixivId: { } id } => $"https://www.pixiv.net/artworks/{id}",
            { DanbooruId: { } id } => $"https://danbooru.donmai.us/posts/{id}",
            { YandereId: { } id } => $"https://yande.re/post/show/{id}",
            { GelbooruId: { } id } => $"https://gelbooru.com/index.php?page=post&s=view&id={id}",
            _ => null
        };

    private static SauceNaoIndex ParseSauceNaoIndex(string? index)
    {
        if (string.IsNullOrWhiteSpace(index))
            return SauceNaoIndex.All;

        if (long.TryParse(index, out var numeric))
            return (SauceNaoIndex) numeric;

        if (Enum.TryParse<SauceNaoIndex>(index, ignoreCase: true, out var value))
            return value;

        return Normalize(index) switch
        {
            "all" => SauceNaoIndex.All,
            "pixiv" => SauceNaoIndex.Pixiv,
            "pixivhistorical" => SauceNaoIndex.PixivHistorical,
            "danbooru" => SauceNaoIndex.Danbooru,
            "yandere" => SauceNaoIndex.YandeRe,
            "gelbooru" => SauceNaoIndex.Gelbooru,
            "sankaku" => SauceNaoIndex.Sankaku,
            "twitter" => SauceNaoIndex.Twitter,
            _ => throw new PixevalMcpException("SauceNAO index is not supported.")
        };

        static string Normalize(string value) =>
            value.Trim()
                .Replace(".", "", StringComparison.Ordinal)
                .Replace("-", "", StringComparison.Ordinal)
                .Replace("_", "", StringComparison.Ordinal)
                .ToLowerInvariant();
    }
}

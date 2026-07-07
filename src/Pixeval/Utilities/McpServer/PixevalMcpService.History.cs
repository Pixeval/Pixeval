// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mako.Model;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Mcp;
using Pixeval.Mcp.Dtos;
using Pixeval.Models.Database;
using Pixeval.Models.Database.Managers;
using Pixeval.Models.Filters;

namespace Pixeval.Utilities.McpServer;

public sealed partial class PixevalMcpService
{
    private const int MaxHistoryLimit = 100;

    public Task<PixevalHistoryListDto> HistoryAsync(
        PixevalHistoryType type,
        int skip,
        int limit,
        string? keyword,
        string? workFilter,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var typeName = JsonNamingPolicy.SnakeCaseLower.ConvertName(type.ToString());
        var normalizedSkip = int.Max(0, skip);
        var normalizedLimit = int.Clamp(limit, 1, MaxHistoryLimit);
        return Task.FromResult(type switch
        {
            PixevalHistoryType.Search => GetSearchHistory(typeName, normalizedSkip, normalizedLimit, keyword),
            PixevalHistoryType.Browse => GetArtworkHistory<BrowseHistoryPersistentManager, BrowseHistoryEntry>(
                typeName,
                normalizedSkip,
                normalizedLimit,
                keyword,
                workFilter,
                cancellationToken),
            PixevalHistoryType.WatchLater => GetArtworkHistory<WatchLaterPersistentManager, WatchLaterEntry>(
                typeName,
                normalizedSkip,
                normalizedLimit,
                keyword,
                workFilter,
                cancellationToken),
            PixevalHistoryType.Download => GetDownloadHistory(
                typeName,
                normalizedSkip,
                normalizedLimit,
                keyword,
                workFilter,
                cancellationToken),
            PixevalHistoryType.WorkSubscription => GetWorkSubscriptionHistory(
                typeName,
                normalizedSkip,
                normalizedLimit,
                keyword),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        });
    }

    private PixevalHistoryListDto GetSearchHistory(
        string type,
        int skip,
        int limit,
        string? keyword)
    {
        var entries = ViewModel.AppServiceProvider.GetRequiredService<SearchHistoryPersistentManager>()
            .Reverse()
            .Where(entry => ContainsKeyword(entry.Value, keyword)
                            || ContainsKeyword(entry.TranslatedName, keyword))
            .ToArray();
        var items = entries.Skip(skip).Take(limit)
            .Select(entry => new PixevalHistoryItemDto(
                entry.HistoryEntryId,
                type,
                entry.Time,
                new PixevalSearchHistoryDto(entry.Value, entry.TranslatedName),
                null,
                null,
                null))
            .ToArray();
        return new(type, skip, limit, entries.Length, items.Length, items);
    }

    private PixevalHistoryListDto GetArtworkHistory<TManager, TEntry>(
        string type,
        int skip,
        int limit,
        string? keyword,
        string? workFilter,
        CancellationToken cancellationToken)
        where TManager : SimplePersistentManager<TEntry>
        where TEntry : ArtworkHistoryEntry, new()
    {
        var filter = CreateHistoryWorkFilter(workFilter, out var filterDto);
        if (filterDto is { IsSuccess: false })
            return new(type, skip, limit, 0, 0, [], filterDto);

        var entries = ViewModel.AppServiceProvider.GetRequiredService<TManager>()
            .Reverse()
            .Where(entry => TryGetArtwork(entry, out var artwork)
                            && MatchesArtworkKeyword(artwork, keyword)
                            && filter(artwork))
            .ToArray();
        var items = entries.Skip(skip)
            .Take(limit)
            .Select(entry => CreateArtworkHistoryItem(type, entry, null))
            .OfType<PixevalHistoryItemDto>()
            .ToArray();
        cancellationToken.ThrowIfCancellationRequested();
        return new(type, skip, limit, entries.Length, items.Length, items, filterDto);
    }

    private PixevalHistoryListDto GetDownloadHistory(
        string type,
        int skip,
        int limit,
        string? keyword,
        string? workFilter,
        CancellationToken cancellationToken)
    {
        var filter = CreateHistoryWorkFilter(workFilter, out var filterDto);
        if (filterDto is { IsSuccess: false })
            return new(type, skip, limit, 0, 0, [], filterDto);

        var entries = ViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>()
            .Reverse()
            .Where(task =>
                task.DatabaseEntry is { } entry
                && TryGetArtwork(entry, out var artwork)
                && (ContainsKeyword(entry.Destination, keyword)
                    || ContainsKeyword(entry.ErrorMessage, keyword)
                    || MatchesArtworkKeyword(artwork, keyword))
                && filter(artwork))
            .Select(task => task.DatabaseEntry)
            .ToArray();
        var items = entries.Skip(skip)
            .Take(limit)
            .Select(entry => CreateArtworkHistoryItem(
                type,
                entry,
                new PixevalDownloadHistoryDto(
                    entry.Destination,
                    entry.State.ToString(),
                    entry.FormatToken,
                    entry.ErrorMessage,
                    entry.WorkSubscriptionId)))
            .OfType<PixevalHistoryItemDto>()
            .ToArray();
        cancellationToken.ThrowIfCancellationRequested();
        return new(type, skip, limit, entries.Length, items.Length, items, filterDto);
    }

    private PixevalHistoryListDto GetWorkSubscriptionHistory(
        string type,
        int skip,
        int limit,
        string? keyword)
    {
        var entries = ViewModel.AppServiceProvider.GetRequiredService<WorkSubscriptionPersistentManager>()
            .Reverse()
            .Where(entry =>
                ContainsKeyword(entry.Name, keyword)
                || ContainsKeyword(entry.Account, keyword)
                || ContainsKeyword(entry.UserId.ToString(), keyword))
            .ToArray();
        var items = entries.Skip(skip)
            .Take(limit)
            .Select(entry => new PixevalHistoryItemDto(
                entry.HistoryEntryId,
                type,
                null,
                null,
                null,
                null,
                new PixevalWorkSubscriptionDto(
                    entry.HistoryEntryId,
                    entry.UserId,
                    entry.Name,
                    entry.Account,
                    entry.AvatarUrl,
                    entry.SubscriptionType.ToString(),
                    entry.WorkKind.ToString())))
            .ToArray();
        return new(type, skip, limit, entries.Length, items.Length, items);
    }

    private Func<IArtworkInfo, bool> CreateHistoryWorkFilter(
        string? workFilter,
        out PixevalWorkFilterAnalysisDto? filterDto)
    {
        filterDto = null;
        if (string.IsNullOrWhiteSpace(workFilter))
            return static _ => true;

        var analysis = WorkFilterLanguage.Instance.Analyze(workFilter);
        filterDto = ToFilterAnalysisDto(workFilter, workFilter.Length, analysis);
        if (!analysis.IsSuccess || analysis.Query is not { } query)
            return static _ => false;

        return artwork => WorkFilterEvaluator.Filter(artwork, query.Root);
    }

    private PixevalHistoryItemDto? CreateArtworkHistoryItem(
        string type,
        ArtworkHistoryEntry entry,
        PixevalDownloadHistoryDto? download)
    {
        try
        {
            return TryGetArtwork(entry, out var artwork)
                ? new PixevalHistoryItemDto(
                    entry.HistoryEntryId,
                    type,
                    null,
                    null,
                    CreateArtworkDto(entry, artwork),
                    download,
                    null)
                : null;
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to serialize MCP history item {entry.HistoryEntryId}", e);
            return null;
        }
    }

    private static PixevalHistoryArtworkDto CreateArtworkDto(
        ArtworkHistoryEntry entry,
        IArtworkInfo artwork)
    {
        var imageSize = artwork as IImageSize;
        return new(
            artwork.Id,
            artwork is ISerializable serializable ? serializable.SerializeKey : entry.SerializeKey,
            artwork.Title,
            [
                .. artwork.Tags.SelectMany(static tags => tags)
                    .Select(static tag => new PixevalTagDto(tag.Name, tag.TranslatedName))
                    .Distinct()
            ],
            TryGetThumbnailUrl(artwork),
            artwork.CreateDate,
            artwork.SafeRating.ToString(),
            artwork.ImageType.ToString(),
            artwork.IsAiGenerated,
            artwork.TotalFavorite,
            imageSize?.Width,
            imageSize?.Height,
            imageSize is { Width: > 0, Height: > 0 } ? imageSize.AspectRatio : null,
            artwork is WorkBase work ? PixevalWorkDto.FromWork(work) : null);
    }

    private static bool TryGetArtwork(ArtworkHistoryEntry entry, out IArtworkInfo artwork)
    {
        try
        {
            artwork = entry.Entry;
            return true;
        }
        catch
        {
            artwork = null!;
            return false;
        }
    }

    private static string? TryGetThumbnailUrl(IArtworkInfo artwork)
    {
        try
        {
            return artwork.Thumbnails.FirstOrDefault()?.ImageUri.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static bool MatchesArtworkKeyword(IArtworkInfo artwork, string? keyword) =>
        string.IsNullOrWhiteSpace(keyword)
        || ContainsKeyword(artwork.Id, keyword)
        || ContainsKeyword(artwork.Title, keyword)
        || artwork.Authors.Any(author => ContainsKeyword(author.Name, keyword))
        || artwork.Tags.Any(tags => tags.Any(tag =>
            ContainsKeyword(tag.Name, keyword)
            || ContainsKeyword(tag.TranslatedName, keyword)));

    private static bool ContainsKeyword(string? value, string? keyword) =>
        string.IsNullOrWhiteSpace(keyword)
        || value?.Contains(keyword, StringComparison.OrdinalIgnoreCase) is true;
}

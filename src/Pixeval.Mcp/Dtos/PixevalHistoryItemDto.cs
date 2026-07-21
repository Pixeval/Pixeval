// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalHistoryItemDto(
    int HistoryEntryId,
    PixevalHistoryType Type,
    DateTimeOffset? Time,
    PixevalSearchHistoryDto? Search,
    PixevalHistoryArtworkDto? Artwork,
    PixevalDownloadHistoryDto? Download,
    PixevalWorkSubscriptionDto? WorkSubscription);

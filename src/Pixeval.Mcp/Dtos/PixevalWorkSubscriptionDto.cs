// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalWorkSubscriptionDto(
    int HistoryEntryId,
    long UserId,
    string Name,
    string Account,
    string AvatarUrl,
    string SubscriptionType,
    string WorkKind);

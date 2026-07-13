// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

using Pixeval.Mcp;

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalWorkSubscriptionDto(
    int HistoryEntryId,
    long TargetId,
    string Name,
    string Description,
    string ImageUrl,
    PixevalWorkSubscriptionType SubscriptionType,
    PixevalWorkSubscriptionWorkKind WorkKind);

// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalSauceNaoResultDto(
    double Similarity,
    string Index,
    long? PixivId,
    string? DanbooruId,
    string? YandereId,
    string? GelbooruId,
    string? SankakuId,
    string? WebsiteUrl,
    string? PixevalUri,
    PixevalWorkDto? PixivWork);
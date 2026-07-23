// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalSauceNaoResultDto(
    double Similarity,
    string Index,
    long? PixivId,
    long? DanbooruId,
    long? YandereId,
    long? GelbooruId,
    long? SankakuId,
    string? WebsiteUrl,
    string? PixevalUri,
    PixevalWorkDto? PixivWork);

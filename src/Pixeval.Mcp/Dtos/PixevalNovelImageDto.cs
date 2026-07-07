// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalNovelImageDto(
    long NovelImageId,
    int Sl,
    string ThumbnailUrl,
    string OriginalUrl);
// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalUserProfileDto(
    int TotalFollowUsers,
    int TotalIllustrations,
    int TotalManga,
    int TotalNovels,
    int TotalPublicIllustrationBookmarks,
    string Webpage,
    string TwitterUrl,
    string? BackgroundImageUrl);
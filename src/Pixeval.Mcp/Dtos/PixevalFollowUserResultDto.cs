// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalFollowUserResultDto(
    bool Success,
    string Message,
    bool IsFollowed,
    PixevalUserDto User);
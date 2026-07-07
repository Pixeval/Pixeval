// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalUserListDto(
    int Count,
    IReadOnlyList<PixevalUserDto> Users);
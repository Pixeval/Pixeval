// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalStatusDto(
    bool LoggedIn,
    string AppVersion,
    string TargetFilter,
    PixevalUserDto? User)
{
    public static PixevalStatusDto FromRuntime(IPixevalMcpRuntime runtime) =>
        new(
            runtime.CurrentUser is not null,
            runtime.AppVersion,
            runtime.TargetFilter,
            runtime.CurrentUser is { } user ? PixevalUserDto.FromUserInfo(user) : null);
}

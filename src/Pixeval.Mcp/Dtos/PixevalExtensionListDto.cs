// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalExtensionListDto(
    int Count,
    string CurrentSdkVersion,
    string? NativeLibraryExtension,
    IReadOnlyList<PixevalExtensionTypeStatisticDto> TypeStatistics,
    IReadOnlyList<PixevalExtensionHostDto> Hosts);
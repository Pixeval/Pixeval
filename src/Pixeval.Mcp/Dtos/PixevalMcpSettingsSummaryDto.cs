// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalMcpSettingsSummaryDto(
    bool EnableServer,
    ushort Port,
    bool EnableWriteTools,
    int MaxBinaryResourceMegabytes);

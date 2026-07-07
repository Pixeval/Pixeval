// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalMcpDownloadTaskDto(
    int QueueIndex,
    string? WorkId,
    string? WorkTitle,
    string Destination,
    string OpenLocalDestination,
    string State,
    double ProgressPercentage,
    int ActiveCount,
    int CompletedCount,
    int ErrorCount,
    string? ErrorMessage);
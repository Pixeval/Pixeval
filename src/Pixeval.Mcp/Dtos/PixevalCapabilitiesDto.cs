// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalCapabilitiesDto(
    bool LoggedIn,
    bool WriteToolsEnabled,
    bool ThumbnailResourcesEnabled,
    ushort Port,
    string Endpoint,
    int MaxBinaryResourceMegabytes)
{
    public static PixevalCapabilitiesDto FromRuntime(IPixevalMcpRuntime runtime) =>
        new(
            runtime.CurrentUser is not null,
            runtime.EnableWriteTools,
            true,
            runtime.Port,
            $"http://127.0.0.1:{runtime.Port}{PixevalMcpHttpServer.DefaultPath}",
            runtime.MaxBinaryResourceMegabytes);
}

// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

namespace Pixeval.Mcp;

public sealed class PixevalMcpHttpServer(IPixevalMcpRuntime runtime, int port) : IAsyncDisposable
{
    public const int DefaultPort = 52163;

    public const string DefaultPath = "/mcp";

    private static readonly TimeSpan _DefaultStopTimeout = TimeSpan.FromSeconds(2);

    private WebApplication? _application;

    public int Port { get; } = port;

    public Uri Endpoint { get; } = new($"http://127.0.0.1:{port}{DefaultPath}");

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_application is not null)
            return;

        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { ApplicationName = typeof(PixevalMcpHttpServer).Assembly.GetName().Name });

        builder.Logging.ClearProviders();
        builder.WebHost.UseKestrel(options => options.Listen(IPAddress.Loopback, Port));

        builder.Services
            .AddSingleton(runtime)
            .AddSingleton<PixevalMcpCursorStore>()
            .AddSingleton<PixevalMcpReadTools>()
            .AddSingleton<PixevalMcpWriteTools>()
            .AddSingleton<PixevalMcpResources>();

        builder.Services
            .AddMcpServer(options =>
            {
                options.ServerInfo = new Implementation
                {
                    Name = $"{nameof(Pixeval)} MCP",
                    Version = runtime.AppVersion
                };
                options.ServerInstructions =
                    $"Expose the running {nameof(Pixeval)} desktop session as local MCP tools. Tools follow {nameof(Pixeval)}'s current account, content filter, network settings, and MCP permission settings.";
            })
            .WithHttpTransport(options => options.Stateless = true)
            .WithTools<PixevalMcpReadTools>(PixevalMcpResult.JsonOptions)
            .WithTools<PixevalMcpWriteTools>(PixevalMcpResult.JsonOptions)
            .WithResources<PixevalMcpResources>();

        _application = builder.Build();
        _ = _application.MapMcp(DefaultPath);
        await _application.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task StopAsync(TimeSpan timeout)
    {
        if (_application is null)
            return;

        await using var application = _application;
        _application = null;
        using var cts = new CancellationTokenSource(timeout);
        await application.StopAsync(cts.Token).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (_application is null)
            return;

        await StopAsync(_DefaultStopTimeout).ConfigureAwait(false);
    }
}

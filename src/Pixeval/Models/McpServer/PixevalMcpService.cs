// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Mako;
using Mako.Global.Enum;
using Mako.Model;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.AppManagement;
using Pixeval.Download;
using Pixeval.Mcp;
using Pixeval.Mcp.Dtos;
using Pixeval.Models.Download;
using Pixeval.Models.Download.Tasks;
using Pixeval.Utilities;

namespace Pixeval.Models.McpServer;

public sealed partial class PixevalMcpService(AppViewModel appViewModel, FileLogger logger)
    : IPixevalMcpService, IPixevalMcpRuntime
{
    private static readonly TimeSpan _StopTimeout = TimeSpan.FromSeconds(2);

    private readonly SemaphoreSlim _lifetimeLock = new(1, 1);
    private PixevalMcpHttpServer? _server;
    private ushort? _serverPort;
    private bool _disposed;

    private AppViewModel ViewModel => appViewModel;

    private FileLogger Logger => logger;

    private McpSettingsGroup Settings => appViewModel.AppSettings.McpSettings;

    public string AppVersion => AppInfo.AppVersion.CurrentVersionShortText;

    public string TargetFilter => appViewModel.AppSettings.BrowsingExperienceSettings.TargetFilter.ToString();

    public TokenUser? CurrentUser => MakoClient.Me ?? appViewModel.GetCurrentLoginUser()?.TokenUser;

    public MakoClient MakoClient => appViewModel.MakoClient;

    public HttpClient ImageHttpClient => MakoClient.GetImageDownloadClient();

    public ushort Port => Settings.Port;

    public bool EnableWriteTools => Settings.EnableWriteTools;

    public int MaxBinaryResourceMegabytes =>
        int.Clamp(Settings.MaxBinaryResourceMegabytes, 1, McpSettingsGroup.MaxBinaryResourceMegabytesLimit);

    public Uri? Endpoint => _server?.Endpoint;

    public Task StartAsync(CancellationToken cancellationToken = default) =>
        ApplySettingsAsync(cancellationToken);

    public async Task ApplySettingsAsync(CancellationToken cancellationToken = default)
    {
        await _lifetimeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_disposed)
                return;

            await ApplySettingsCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _lifetimeLock.Release();
        }
    }

    private async Task ApplySettingsCoreAsync(CancellationToken cancellationToken)
    {
        var port = Port;
        if (!Settings.EnableServer)
        {
            await StopCoreAsync().ConfigureAwait(false);
            return;
        }

        if (_server is not null && _serverPort == port)
            return;

        if (_server is not null)
            await StopCoreAsync().ConfigureAwait(false);

        await StartCoreAsync(port, cancellationToken).ConfigureAwait(false);
    }

    private async Task StartCoreAsync(ushort port, CancellationToken cancellationToken)
    {
        var server = new PixevalMcpHttpServer(this, port);
        try
        {
            await server.StartAsync(cancellationToken).ConfigureAwait(false);
            _server = server;
            _serverPort = port;
            logger.LogInformation($"Pixeval MCP server started at {server.Endpoint}", null);
        }
        catch (Exception e)
        {
            logger.LogError("Failed to start Pixeval MCP server", e);
            await server.DisposeAsync().ConfigureAwait(false);
        }
    }

    public async Task StopAsync()
    {
        await _lifetimeLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_disposed)
                return;

            await StopCoreAsync().ConfigureAwait(false);
        }
        finally
        {
            _lifetimeLock.Release();
        }
    }

    private async Task StopCoreAsync()
    {
        if (_server is not { } server)
        {
            _serverPort = null;
            return;
        }

        _server = null;
        _serverPort = null;
        try
        {
            await server.StopAsync(_StopTimeout).ConfigureAwait(false);
            logger.LogInformation("Pixeval MCP server stopped", null);
        }
        catch (OperationCanceledException e)
        {
            logger.LogError("Timed out while stopping Pixeval MCP server", e);
        }
        catch (Exception e)
        {
            logger.LogError("Failed to stop Pixeval MCP server", e);
        }
        finally
        {
            await server.DisposeAsync().ConfigureAwait(false);
        }
    }

    public void EnsureLoggedIn()
    {
        if (appViewModel.GetCurrentLoginUser() is not { RefreshToken.Length: > 0 })
            throw new PixevalMcpException(
                "Pixeval has no active Pixiv account. Log in with the Pixeval desktop app first.");
    }

    public void EnsureWriteToolsEnabled()
    {
        if (!Settings.EnableWriteTools)
            throw new PixevalMcpException("Pixeval MCP write tools are disabled in settings.");
    }

    public async Task<PixevalMcpDownloadTaskDto> QueueDownloadAsync(
        SimpleWorkType workType,
        long id,
        CancellationToken cancellationToken)
    {
        EnsureWriteToolsEnabled();
        EnsureLoggedIn();

        var destination = appViewModel.AppSettings.DownloadSettings.DownloadPathMacro;
        var task = workType is SimpleWorkType.Novel
            ? await CreateNovelDownloadTaskAsync(id, destination, cancellationToken).ConfigureAwait(false)
            : await CreateIllustrationDownloadTaskAsync(id, destination, cancellationToken).ConfigureAwait(false);

        var manager = appViewModel.HistoryPersistHelper.DownloadManager;
        return RunOnUiThread(() =>
        {
            manager.QueueTask(task);
            return ToDownloadTaskDto(task, manager.QueuedTasks.IndexOf(task));
        });
    }

    public IReadOnlyList<PixevalMcpDownloadTaskDto> DownloadTasks() =>
    [
        .. appViewModel.HistoryPersistHelper.DownloadManager.QueuedTasks.Select(static (task, index) =>
            ToDownloadTaskDto(task, index))
    ];

    public void LogToolException(string toolName, Exception exception) =>
        logger.LogError($"MCP tool failed: {toolName}", exception);

    public async ValueTask DisposeAsync()
    {
        await _lifetimeLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_disposed)
                return;

            _disposed = true;
            await StopCoreAsync().ConfigureAwait(false);
        }
        finally
        {
            _lifetimeLock.Release();
        }
    }

    private async Task<IDownloadTaskGroup> CreateIllustrationDownloadTaskAsync(
        long id,
        string destination,
        CancellationToken cancellationToken)
    {
        var illustration = await GetIllustrationAsync(id, cancellationToken).ConfigureAwait(false);
        if (illustration.IsPicGif && illustration is ISingleAnimatedImage { MultiImageUris: not null } animatedImage)
            await animatedImage.MultiImageUris.TryPreloadListAsync(animatedImage).ConfigureAwait(false);

        var factory = appViewModel.AppServiceProvider.GetRequiredService<IllustrationDownloadTaskFactory>();
        return factory.Create(illustration, destination);
    }

    private async Task<IDownloadTaskGroup> CreateNovelDownloadTaskAsync(
        long id,
        string destination,
        CancellationToken cancellationToken)
    {
        var novel = await GetNovelAsync(id, cancellationToken).ConfigureAwait(false);
        var content = await MakoClient.GetNovelContentAsync(id).WaitAsync(cancellationToken).ConfigureAwait(false);
        var factory = appViewModel.AppServiceProvider.GetRequiredService<NovelDownloadTaskFactory>();
        return factory.Create(novel, destination, content);
    }

    private static PixevalMcpDownloadTaskDto ToDownloadTaskDto(IDownloadTaskGroupBase task, int queueIndex)
    {
        var entry = task is IDownloadTaskGroup group ? group.DatabaseEntry.Entry : null;
        return new(
            queueIndex,
            entry?.Id,
            entry?.Title,
            task.Destination,
            task.OpenLocalDestination,
            task.CurrentState.ToString(),
            task.ProgressPercentage,
            task.ActiveCount,
            task.CompletedCount,
            task.ErrorCount,
            task.ErrorMessage);
    }

    private static PixevalMcpDownloadTaskDto ToDownloadTaskDto(IDownloadTaskGroupBase task) =>
        ToDownloadTaskDto(task, -1);
}

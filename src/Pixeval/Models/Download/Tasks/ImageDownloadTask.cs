// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Download;
using Pixeval.Extensions.Common;
using Pixeval.Models.Extensions;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;
using Pixeval.Utilities.IO.Caching;
using Pixeval.ViewModels;

namespace Pixeval.Models.Download.Tasks;

/// <summary>
/// 下载图片的任务，所有复杂的图片下载任务都会从这个类衍生
/// </summary>
public partial class ImageDownloadTask : ViewModelBase, ISingleDownloadTaskBase, IProgress<double>, IDisposable
{
    private const int MaxTimeoutRetries = 2;

    private readonly SemaphoreSlim _downloadLock = new(1, 1);

    public ImageDownloadTask(Uri uri, string destination, DownloadState initState = DownloadState.Queued)
    {
        Uri = uri;
        Destination = destination;
        CurrentState = initState;
        if (initState is DownloadState.Completed or DownloadState.Cancelled or DownloadState.Error)
            ProgressPercentage = 100;
        DownloadStartedAsync += DownloadStartedAsyncOverride;
        DownloadStoppedAsync += DownloadStoppedAsyncOverride;
        DownloadErrorAsync += DownloadErrorAsyncOverride;
        AfterDownloadAsync += InvokeAfterDownloadAsyncOverride;
    }

    protected virtual Task DownloadStartedAsyncOverride(ImageDownloadTask sender) => Task.CompletedTask;

    protected virtual Task DownloadStoppedAsyncOverride(ImageDownloadTask sender) => Task.CompletedTask;

    protected virtual Task DownloadErrorAsyncOverride(ImageDownloadTask sender)
    {
        if (File.Exists(DownloadTempDestination))
            File.Delete(DownloadTempDestination);
        if (_hasCommittedDestination && File.Exists(Destination))
            File.Delete(Destination);
        return Task.CompletedTask;
    }

    protected virtual Task AfterDownloadAsyncOverride(ImageDownloadTask sender, CancellationToken token = default) => Task.CompletedTask;

    private Task InvokeAfterDownloadAsyncOverride(ImageDownloadTask sender, CancellationToken token) =>
        WasDownloadSkipped ? Task.CompletedTask : AfterDownloadAsyncOverride(sender, token);

    public Uri Uri { get; }

    public string Destination { get; }

    private string DownloadTempDestination => Destination + IoHelper.PixevalTempExtension;

    internal bool WasDownloadSkipped { get; private set; }

    protected virtual bool OverwriteDownloadedFile =>
        App.AppViewModel.AppSettings.DownloadSettings.OverwriteDownloadedFile;

    [ObservableProperty]
    public partial DownloadState CurrentState { get; protected set; }

    [ObservableProperty]
    public partial double ProgressPercentage { get; protected set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; protected set; }

    [ObservableProperty]
    public partial bool IsProcessing { get; private set; }

    protected CancellationTokenSource CancellationTokenSource { get; private set; } = new();

    private bool _isRunning;

    private bool _isDisposed;

    private bool _hasCommittedDestination;

    private async Task SetRunningAsync(bool value, bool suppressDownloadStartedAsync = false)
    {
        if (value == _isRunning)
            return;
        _isRunning = value;
        if (!value)
            await DownloadStoppedAsync.Invoke(this);
        else if (!suppressDownloadStartedAsync)
            await DownloadStartedAsync.Invoke(this);
    }

    private async Task PendingCompleteAsync(CancellationToken token)
    {
        ProgressPercentage = 100;
        // CurrentState = DownloadState.Pending;
        CurrentState = DownloadState.Completed;
        await Task.Run(() => AfterDownloadAsync.Invoke(this, token), token);
    }

    private async Task CommitDownloadedFileAsync(bool overwrite, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        _hasCommittedDestination = DownloadTaskFileHelper.CommitDownloadedFile(
            DownloadTempDestination,
            Destination,
            overwrite);
        WasDownloadSkipped = !_hasCommittedDestination;
        await PendingCompleteAsync(token);
    }

    private async Task SetErrorAsync(Exception ex)
    {
        ErrorMessage = ex.ToString();
        CurrentState = DownloadState.Error;
        await DownloadErrorAsync.Invoke(this);
    }

    public virtual async Task StartAsync(HttpClient httpClient)
    {
        if (_isDisposed || CurrentState is not DownloadState.Queued)
            return;

        var token = CancellationTokenSource.Token;
        try
        {
            // A resumed run must wait for the cancelled run to close its temporary file.
            await _downloadLock.WaitAsync(token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            return;
        }

        try
        {
            if (CurrentState is DownloadState.Queued && !token.IsCancellationRequested)
                await RunAsync(httpClient, token);
        }
        finally
        {
            _ = _downloadLock.Release();
        }
    }

    private async Task RunAsync(HttpClient httpClient, CancellationToken token)
    {
        try
        {
            CurrentState = DownloadState.Running;
            await SetRunningAsync(true);
            token.ThrowIfCancellationRequested();
            ProgressPercentage = 0;
            WasDownloadSkipped = false;
            _hasCommittedDestination = false;
            var overwrite = OverwriteDownloadedFile;

            // Partial downloads are never resumed; a new run always starts from a known empty file.
            if (File.Exists(DownloadTempDestination))
                File.Delete(DownloadTempDestination);
            if (DownloadTaskFileHelper.ShouldSkipExistingFile(Destination, overwrite))
            {
                WasDownloadSkipped = true;
                await PendingCompleteAsync(token);
                return;
            }

            if (Uri.Scheme is "http" or "https")
            {
                if (CacheHelper.TryGetStream(Uri.OriginalString) is { } stream)
                {
                    await using (var fs = FileHelper.OpenAsyncWriteCreateParent(DownloadTempDestination))
                        await stream.CopyToAsync(fs, token);
                    await CommitDownloadedFileAsync(overwrite, token);
                    return;
                }

                if (GetExtensionService().ActiveDownloaders.FirstOrDefault() is { } downloader)
                {
                    var notifier = new ProgressNotifier(this);
                    FileHelper.CreateParentDirectory(DownloadTempDestination);
                    downloader.Download(notifier, Uri.OriginalString, DownloadTempDestination);
                    while (!notifier.Finished)
                        await Task.Delay(1000, token);
                    token.ThrowIfCancellationRequested();
                    if (notifier.Exception is null)
                        await CommitDownloadedFileAsync(overwrite, token);
                    else
                        await SetErrorAsync(notifier.Exception);
                    return;
                }
            }

            await DownloadDirectlyAsync(httpClient, overwrite, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // ignored
        }
        catch (Exception ex)
        {
            if (!token.IsCancellationRequested)
                await SetErrorAsync(ex);
        }
        finally
        {
            await SetRunningAsync(false);
        }
    }

    private async Task DownloadDirectlyAsync(HttpClient httpClient, bool overwrite, CancellationToken token)
    {
        for (var retries = 0; ; retries++)
        {
            token.ThrowIfCancellationRequested();
            Exception? error;
            // Opening with FileMode.Create discards the partial response before retrying.
            await using (var fileStream = FileHelper.OpenAsyncWriteCreateParent(DownloadTempDestination))
                error = await httpClient.DownloadStreamAsync(fileStream, Uri, this, token: token);

            token.ThrowIfCancellationRequested();
            if (error is null)
            {
                await CommitDownloadedFileAsync(overwrite, token);
                return;
            }

            if (retries >= MaxTimeoutRetries || error is not (TimeoutException or OperationCanceledException { InnerException: TimeoutException }))
                ExceptionDispatchInfo.Throw(error);

            ProgressPercentage = 0;
            await Task.Delay(TimeSpan.FromSeconds(retries + 1), token);
        }
    }

    public void Reset()
    {
        if (CurrentState is not (DownloadState.Completed or DownloadState.Error or DownloadState.Cancelled))
            return;
        IsProcessing = true;
        ErrorMessage = null;
        ProgressPercentage = 0;
        WasDownloadSkipped = false;
        _hasCommittedDestination = false;
        Delete();
        if (CancellationTokenSource.IsCancellationRequested)
        {
            CancellationTokenSource.Dispose();
            CancellationTokenSource = new();
        }
        CurrentState = DownloadState.Queued;
        DownloadTryReset?.Invoke(this);
        IsProcessing = false;
    }

    public void Pause()
    {
        if (CurrentState is not (DownloadState.Queued or DownloadState.Running))
            return;
        IsProcessing = true;
        CancellationTokenSource.Cancel();
        CurrentState = DownloadState.Paused;
        DownloadPaused?.Invoke(this);
        IsProcessing = false;
    }

    public void Resume()
    {
        if (CurrentState is not DownloadState.Paused)
            return;
        IsProcessing = true;
        if (CancellationTokenSource.IsCancellationRequested)
        {
            CancellationTokenSource.Dispose();
            CancellationTokenSource = new();
        }
        CurrentState = DownloadState.Queued;
        DownloadTryResume?.Invoke(this);
        IsProcessing = false;
    }

    public void Cancel()
    {
        if (CurrentState is not (DownloadState.Paused or DownloadState.Pending or DownloadState.Running or DownloadState.Queued))
            return;
        IsProcessing = true;
        CancellationTokenSource.Cancel();
        CurrentState = DownloadState.Cancelled;
        DownloadCancelled?.Invoke(this);
        IsProcessing = false;
    }

    public void Delete()
    {
        if (File.Exists(Destination))
            File.Delete(Destination);

        if (File.Exists(DownloadTempDestination))
            File.Delete(DownloadTempDestination);
    }

    public string OpenLocalDestination => Destination;

    /// <summary>
    /// 任务开始时会触发一次
    /// </summary>
    public event Func<ImageDownloadTask, Task> DownloadStartedAsync;

    /// <summary>
    /// 任务停止（包括完成时）会触发一次
    /// </summary>
    public event Func<ImageDownloadTask, Task> DownloadStoppedAsync;

    /// <summary>
    /// 任务出现错误时会触发一次
    /// </summary>
    public event Func<ImageDownloadTask, Task> DownloadErrorAsync;

    public event Action<ImageDownloadTask>? DownloadTryResume;

    public event Action<ImageDownloadTask>? DownloadTryReset;

    public event Action<ImageDownloadTask>? DownloadPaused;

    public event Action<ImageDownloadTask>? DownloadCancelled;

    public event Func<ImageDownloadTask, CancellationToken, Task> AfterDownloadAsync;

    private static ExtensionService GetExtensionService() =>
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();

    void IProgress<double>.Report(double value)
    {
        if (CurrentState is DownloadState.Running)
            ProgressPercentage = value;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_isDisposed)
            return;

        _isDisposed = true;
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
    }

    [GeneratedComClass]
    private partial class ProgressNotifier(IProgress<double> progress)
        : IProgressNotifier
    {
        public bool Finished { get; private set; }

        public Exception? Exception { get; private set; }

        public void ProgressChanged(double value) => progress.Report(value);

        public void Completed() => Finished = true;

        public void Aborted(IException? exception)
        {
            Finished = true;
            Exception = exception?.ToException() ?? new OperationCanceledException();
        }
    }
}

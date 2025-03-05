// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.AppManagement;
using Pixeval.Util.IO;
using Pixeval.Utilities;

namespace Pixeval.Download.Models;

/// <summary>
/// 下载图片的任务，所有复杂的图片下载任务都会从这个类衍生
/// </summary>
public partial class ImageDownloadTask : ObservableObject, IDownloadTaskBase, IProgress<double>, IDisposable
{
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
        AfterDownloadAsync += AfterDownloadAsyncOverride;
    }

    protected virtual Task DownloadStartedAsyncOverride(ImageDownloadTask sender) => Task.CompletedTask;

    protected virtual Task DownloadStoppedAsyncOverride(ImageDownloadTask sender) => Task.CompletedTask;

    protected virtual Task DownloadErrorAsyncOverride(ImageDownloadTask sender)
    {
        if (File.Exists(Destination))
            File.Delete(Destination);
        return Task.CompletedTask;
    }

    protected virtual Task AfterDownloadAsyncOverride(ImageDownloadTask sender, CancellationToken token = default) => Task.CompletedTask;

    public Stream? Stream { get; init; }

    public Uri Uri { get; }

    public string Destination { get; }

    private string DownloadTempDestination => Destination + ".pixevaldownloading";

    [ObservableProperty]
    public partial DownloadState CurrentState { get; set; }

    [ObservableProperty]
    public partial double ProgressPercentage { get; set; }

    [ObservableProperty]
    public partial Exception? ErrorCause { get; set; }

    [ObservableProperty]
    public partial bool IsProcessing { get; set; }

    protected CancellationTokenSource CancellationTokenSource { get; private set; } = new();

    private bool _isRunning;

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

    /// <summary>
    /// 检查文件是否存在，如果存在则根据设置决定是否覆盖
    /// </summary>
    /// <returns><see langword="true"/>表示已存在不需要下载，<see langword="false"/>表示需要下载图片</returns>
    private bool ValidateExistence()
    {
        if (File.Exists(Destination))
            if (App.AppViewModel.AppSettings.OverwriteDownloadedFile)
                File.Delete(Destination);
            else
                return true;

        var path = null as string;
        if (Uri.IsFile)
            path = Uri.OriginalString;
        else if (Uri.Scheme is "ms-appx")
            path = AppInfo.ApplicationUriToPath(Uri);
        if (path is not null)
        {
            File.Copy(path, Destination);
            return true;
        }
        return false;
    }

    private async Task PendingCompleteAsync()
    {
        ProgressPercentage = 100;
        // CurrentState = DownloadState.Pending;
        CurrentState = DownloadState.Completed;
        await Task.Run(async () => await AfterDownloadAsync.Invoke(this, CancellationTokenSource.Token), CancellationTokenSource.Token);
    }

    private async Task SetErrorAsync(Exception ex)
    {
        ErrorCause = ex;
        CurrentState = DownloadState.Error;
        await DownloadErrorAsync.Invoke(this);
    }

    public virtual async Task StartAsync(HttpClient httpClient, bool resumeBreakpoint = false)
    {
        if (CurrentState is not DownloadState.Queued)
            return;
        try
        {
            CurrentState = DownloadState.Running;
            await SetRunningAsync(true);
            IoHelper.CreateParentDirectories(Destination);
            if (Stream is not null)
            {
                await using (var fs = OpenCreate(DownloadTempDestination))
                    await Stream.CopyToAsync(fs, CancellationTokenSource.Token);
                File.Move(DownloadTempDestination, Destination);
                await PendingCompleteAsync();
                return;
            }
            if (!resumeBreakpoint && ValidateExistence())
            {
                await PendingCompleteAsync();
                return;
            }

            Exception? ex;
            await using (var fileStream = OpenCreate(DownloadTempDestination))
                ex = await httpClient.DownloadStreamAsync(fileStream, Uri, this, fileStream.Length, cancellationToken: CancellationTokenSource.Token);
            switch (ex)
            {
                case null:
                    File.Move(DownloadTempDestination, Destination);
                    await PendingCompleteAsync();
                    break;
                case TaskCanceledException: break;
                default: await SetErrorAsync(ex); break;
            }
        }
        catch (TaskCanceledException)
        {
            // ignored
        }
        catch (Exception ex)
        {
            await SetErrorAsync(ex);
        }
        finally
        {
            await SetRunningAsync(false);
        }

        return;

        static FileStream OpenCreate(string path) => File.Open(path, new FileStreamOptions
        {
            BufferSize = 1 << 20,
            Mode = FileMode.OpenOrCreate,
            Access = FileAccess.Write,
            Share = FileShare.Read,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan
        });
    }

    public void TryReset()
    {
        if (CurrentState is not (DownloadState.Completed or DownloadState.Error or DownloadState.Cancelled))
            return;
        IsProcessing = true;
        ErrorCause = null;
        ProgressPercentage = 0;
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
        CancellationTokenSource.TryCancel();
        CurrentState = DownloadState.Paused;
        DownloadPaused?.Invoke(this);
        IsProcessing = false;
    }

    public void TryResume()
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
        CancellationTokenSource.TryCancel();
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

    void IProgress<double>.Report(double value) => ProgressPercentage = value;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        CancellationTokenSource.Dispose();
    }
}

#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/ImageDownloadTask.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.AppManagement;
using Pixeval.Util;
using Pixeval.Util.IO;

namespace Pixeval.Download.Models;

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

    [ObservableProperty] private DownloadState _currentState;

    [ObservableProperty] private double _progressPercentage;

    [ObservableProperty] private Exception? _errorCause;

    [ObservableProperty] private bool _isProcessing;

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

    private async Task<bool> ValidateExistenceAsync()
    {
        var path = null as string;
        if (App.AppViewModel.AppSettings.UseFileCache && await App.AppViewModel.Cache.TryGetAsync<FileInfo>(MakoHelper.GetOriginalCacheKey(Uri.OriginalString)) is { } fileInfo)
            path = fileInfo.FullName;
        if (Uri.IsFile)
            path = Uri.OriginalString;
        else if (Uri.Scheme is "ms-appx")
            path = AppInfo.ApplicationUriToPath(Uri);
        if (path is not null)
        {
            if (File.Exists(Destination))
                File.Delete(Destination);
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

    public async Task StartAsync(HttpClient httpClient, bool resumeBreakpoint = false)
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
                await using (var fs = OpenCreate(Destination))
                    await Stream.CopyToAsync(fs, CancellationTokenSource.Token);
                await PendingCompleteAsync();
                return;
            }
            if (!resumeBreakpoint && await ValidateExistenceAsync())
            {
                await PendingCompleteAsync();
                return;
            }

            Exception? ex;
            await using (var fileStream = OpenCreate(Destination))
                ex = await httpClient.DownloadStreamAsync(fileStream, Uri, CancellationTokenSource.Token, this, fileStream.Length);
            switch (ex)
            {
                case null: await PendingCompleteAsync(); break;
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
        CancellationTokenSource.Cancel();
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
        CancellationTokenSource.Cancel();
        CurrentState = DownloadState.Cancelled;
        DownloadCancelled?.Invoke(this);
        IsProcessing = false;
    }

    public void Delete()
    {
        if (File.Exists(Destination))
            File.Delete(Destination);
    }

    public string OpenLocalDestination => Destination;

    public event Func<ImageDownloadTask, Task> DownloadStartedAsync;

    public event Func<ImageDownloadTask, Task> DownloadStoppedAsync;

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

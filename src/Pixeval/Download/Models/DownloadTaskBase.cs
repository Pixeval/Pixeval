#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/ObservableDownloadTask.cs
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Database;
using Pixeval.Database.Managers;
using Pixeval.Download.Macros;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;
using QuestPDF.Fluent;
using SixLabors.ImageSharp;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

public abstract class DownloadTaskBase(DownloadHistoryEntry entry) : ObservableObject, IProgress<double>, IIdEntry
{
    public abstract IWorkViewModel ViewModel { get; }
    private Exception? _errorCause;
    private double _progressPercentage = entry.State is DownloadState.Completed ? 100 : 0;

    /// <summary>
    /// 只有<see cref="CurrentState"/>是<see cref="DownloadState.Running"/>或<see cref="DownloadState.Paused"/>值有效
    /// </summary>
    public double ProgressPercentage
    {
        get => _progressPercentage;
        private set
        {
            if (value.Equals(_progressPercentage))
                return;
            _progressPercentage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 文件的真正下载地址，若<see cref="IsFolder"/>为假，则一定只有一个元素
    /// </summary>
    public abstract IReadOnlyList<string> ActualDestinations { get; }

    public string ActualDestination => ActualDestinations[0];

    /// <summary>
    /// 是否是一个文件夹包含的下载内容
    /// </summary>
    public abstract bool IsFolder { get; }

    public DownloadHistoryEntry DatabaseEntry { get; } = entry;

    public long Id => DatabaseEntry.Entry.Id;

    public DownloadItemType Type => DatabaseEntry.Type;

    /// <inheritdoc cref="DownloadHistoryEntry.Destination"/>
    public string Destination => DatabaseEntry.Destination;

    public CancellationHandle CancellationHandle { get; set; } = new();

    public TaskCompletionSource Completion { get; private set; } = new();

    public DownloadState CurrentState
    {
        get => DatabaseEntry.State;
        set => SetProperty(DatabaseEntry.State, value, DatabaseEntry, (entry, state) =>
        {
            entry.State = state;
            if (value is DownloadState.Completed)
            {
                ProgressPercentage = 100;
                Completion.SetResult();
            }
        });
    }

    public Exception? ErrorCause
    {
        get => _errorCause;
        set
        {
            if (Equals(value, _errorCause))
                return;
            _errorCause = value;
            OnPropertyChanged();

            DatabaseEntry.ErrorCause = value?.ToString();
            if (value is not null)
            {
                CurrentState = DownloadState.Error;
                Completion.SetException(value);
            }
        }
    }

    public abstract Task DownloadAsync(Downloader downloadStreamAsync);

    public async Task ResetAsync()
    {
        await IoHelper.DeleteTaskAsync(this);
        ProgressPercentage = 0;
        CurrentState = DownloadState.Queued;
        ErrorCause = null;
        Completion = new TaskCompletionSource();
    }

    public virtual void Report(double value) => ProgressPercentage = value;
}

public delegate Task<Result<Stream>> Downloader(string url, IProgress<double>? progress, CancellationHandle? cancellationHandle);

public interface IDownloadTaskBase
{
    /// <summary>
    /// 只有<see cref="CurrentState"/>是<see cref="DownloadState.Running"/>或<see cref="DownloadState.Paused"/>值有效
    /// </summary>
    double ProgressPercentage { get; }

    DownloadState CurrentState { get; }

    string Destination { get; }

    void TryReset();

    void Pause();

    void TryResume();

    void Cancel();
}

public partial class ImageDownloadTask : ObservableObject, IDownloadTaskBase, IProgress<double>, IDisposable
{
    public ImageDownloadTask(Uri uri, string destination)
    {
        Uri = uri;
        Destination = destination;
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

    protected virtual Task AfterDownloadAsyncOverride(ImageDownloadTask sender) => Task.CompletedTask;

    public Stream? Stream { get; init; }

    public Uri Uri { get; }

    public string Destination { get; }

    [ObservableProperty] private DownloadState _currentState;

    [ObservableProperty] private double _progressPercentage;

    [ObservableProperty] private Exception? _errorCause;

    private CancellationTokenSource CancellationTokenSource { get; } = new();

    private bool _isRunning;

    private async Task SetRunningAsync(bool value, bool suppressDownloadStartedAsync = false)
    {
        if (value == _isRunning)
            return;
        _isRunning = value;
        if (!_isRunning)
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

    private async Task SetCompleteAsync()
    {
        ProgressPercentage = 100;
        await AfterDownloadAsync.Invoke(this);
        CurrentState = DownloadState.Completed;
    }

    private async Task SetErrorAsync(Exception ex)
    {
        ErrorCause = ex;
        CurrentState = DownloadState.Error;
        await DownloadErrorAsync.Invoke(this);
    }

    public async Task StartAsync(HttpClient httpClient, bool resumeBreakpoint = false, bool suppressDownloadStartedAsync = false)
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
                await using var fs = OpenCreate(Destination);
                await SetCompleteAsync();
                return;
            }
            if (!resumeBreakpoint && await ValidateExistenceAsync())
            {
                await SetCompleteAsync();
                return;
            }

            await using var fileStream = OpenCreate(Destination);
            var ex = await httpClient.DownloadStreamAsync(fileStream, Uri, CancellationTokenSource.Token, this, fileStream.Length);
            switch (ex)
            {
                case null: await SetCompleteAsync(); break;
                case TaskCanceledException: break;
                default: await SetErrorAsync(ex); break;
            }
        }
        catch (Exception ex)
        {
            await SetErrorAsync(ex);
        }
        finally
        {
            await SetRunningAsync(false, suppressDownloadStartedAsync);
        }

        return;

        static FileStream OpenCreate(string path) => File.Open(path, new FileStreamOptions
        {
            BufferSize = 1 << 20,
            Mode = FileMode.OpenOrCreate,
            Access = FileAccess.Write,
            Share = FileShare.None,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan
        });
    }

    public void TryReset()
    {
        if (CurrentState is not (DownloadState.Completed or DownloadState.Error))
            return;
        ErrorCause = null;
        ProgressPercentage = 0;
        _ = CancellationTokenSource.TryReset();
        CurrentState = DownloadState.Queued;
        DownloadTryReset?.Invoke(this);
    }

    public void Pause()
    {
        if (CurrentState is not DownloadState.Running)
            return;
        CancellationTokenSource.Cancel();
        CurrentState = DownloadState.Paused;
    }

    public void TryResume()
    {
        if (CurrentState is not DownloadState.Paused)
            return;
        _ = CancellationTokenSource.TryReset();
        CurrentState = DownloadState.Queued;
        DownloadTryResume?.Invoke(this);
    }

    public async Task ResumeAsync(HttpClient httpClient)
    {
        await StartAsync(httpClient, true);
    }

    public void Cancel()
    {
        if (CurrentState is not (DownloadState.Paused or DownloadState.Running or DownloadState.Queued))
            return;
        CancellationTokenSource.Cancel();
        CurrentState = DownloadState.Cancelled;
    }

    public event Func<ImageDownloadTask, Task> DownloadStartedAsync;

    public event Func<ImageDownloadTask, Task> DownloadStoppedAsync;

    public event Func<ImageDownloadTask, Task> DownloadErrorAsync;

    public event Action<ImageDownloadTask>? DownloadTryResume;

    public event Action<ImageDownloadTask>? DownloadTryReset;

    public event Func<ImageDownloadTask, Task> AfterDownloadAsync;

    void IProgress<double>.Report(double value) => ProgressPercentage = value;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        CancellationTokenSource.Dispose();
    }
}

public interface ITaskGroup : IDownloadTaskBase, IIdEntry, INotifyPropertyChanged, INotifyPropertyChanging, IReadOnlyCollection<ImageDownloadTask>, IDisposable
{
}

public abstract class DownloadTaskGroup : ObservableObject, ITaskGroup
{
    public DownloadHistoryEntry DatabaseEntry { get; }

    public long Id => DatabaseEntry.Entry.Id;

    protected DownloadTaskGroup(IWorkEntry entry, string destination, DownloadItemType type)
    {
        DatabaseEntry = new(destination, type, entry);
        PropertyChanged += (sender, e) =>
        {
            var g = sender.To<DownloadTaskGroup>();
            if (e.PropertyName is not nameof(CurrentState))
                return;
            if (g.CurrentState is DownloadState.Running or DownloadState.Paused)
                return;
            g.DatabaseEntry.State = g.CurrentState;
            var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
            manager.Update(g.DatabaseEntry);
        };
        AfterItemDownloadAsync += async _ =>
        {
            if (IsCompleted && AfterAllDownloadAsync is not null)
                await AfterAllDownloadAsync.Invoke(this);
        };
        AfterAllDownloadAsync += AfterAllDownloadAsyncOverride;
    }

    protected abstract Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender);

    /// <inheritdoc cref="DownloadHistoryEntry.Destination"/>
    public string TokenizedDestination => DatabaseEntry.Destination;

    public int Count => TasksSet.Count;

    public IReadOnlyList<string> Destinations => TasksSet.Select(t => t.Destination).ToArray();

    protected IReadOnlyList<ImageDownloadTask> TasksSet => _tasksSet;

    private readonly List<ImageDownloadTask> _tasksSet = [];

    string IDownloadTaskBase.Destination => DatabaseEntry.Destination;

    public void TryReset() => TasksSet.ForEach(t => t.TryReset());

    public void Pause() => TasksSet.ForEach(t => t.Pause());

    public void TryResume() => TasksSet.ForEach(t => t.TryResume());

    public void Cancel() => TasksSet.ForEach(t => t.Cancel());

    public DownloadState CurrentState
    {
        get
        {
            var isError = false;
            var isRunning = false;
            var isPaused = false;
            var isCancelled = false;
            var isQueued = false;
            foreach (var task in TasksSet)
                switch (task.CurrentState)
                {
                    case DownloadState.Queued:
                        isQueued = true;
                        break;
                    case DownloadState.Running:
                        isRunning = true;
                        break;
                    case DownloadState.Paused:
                        if (isCancelled)
                            ThrowHelper.ArgumentOutOfRange(CurrentState);
                        isPaused = true;
                        break;
                    case DownloadState.Cancelled:
                        if (isPaused)
                            ThrowHelper.ArgumentOutOfRange(CurrentState);
                        isCancelled = true;
                        break;
                    case DownloadState.Error:
                        isError = true;
                        break;
                    default:
                        break;
                }

            return isError ? DownloadState.Error :
                isCancelled ? DownloadState.Cancelled :
                isPaused ? DownloadState.Paused :
                isRunning ? DownloadState.Running :
                isQueued ? DownloadState.Queued :
                DownloadState.Completed;
        }
    }

    public event Func<ImageDownloadTask, Task>? ItemDownloadStartedAsync;

    public event Func<ImageDownloadTask, Task>? ItemDownloadStoppedAsync;

    public event Func<ImageDownloadTask, Task>? ItemDownloadErrorAsync;

    public event Func<ImageDownloadTask, Task>? AfterItemDownloadAsync;

    public event Func<DownloadTaskGroup, Task>? AfterAllDownloadAsync;

    protected void AddToTaskSet(ImageDownloadTask task)
    {
        _tasksSet.Add(task);
        task.AfterDownloadAsync += AfterItemDownloadAsync;
        task.DownloadStartedAsync += ItemDownloadStartedAsync;
        task.DownloadStoppedAsync += ItemDownloadStoppedAsync;
        task.DownloadErrorAsync += ItemDownloadErrorAsync;
        task.PropertyChanged += (_, e) => OnPropertyChanged(e.PropertyName switch
        {
            nameof(ImageDownloadTask.CurrentState) => nameof(CurrentState),
            nameof(ImageDownloadTask.ProgressPercentage) => nameof(ProgressPercentage),
            nameof(ImageDownloadTask.ErrorCause) => nameof(ErrorCause),
            _ => ""
        });
    }

    public Exception? ErrorCause => TasksSet.FirstOrDefault(t => t.ErrorCause is not null)?.ErrorCause;

    public bool IsCompleted => TasksSet.All(t => t.CurrentState is DownloadState.Completed);

    public bool IsError => TasksSet.Any(t => t.CurrentState is DownloadState.Error);

    public double ProgressPercentage => TasksSet.Average(t => t.ProgressPercentage);

    public IEnumerator<ImageDownloadTask> GetEnumerator() => TasksSet.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var task in TasksSet)
            task.Dispose();
    }
}

public class MangaDownloadTaskGroup : DownloadTaskGroup
{
    public Illustration Entry => DatabaseEntry.Entry.To<Illustration>();

    public MangaDownloadTaskGroup(Illustration entry, string destination) : base(entry, destination, DownloadItemType.Manga)
    {
        var mangaOriginalUrls = entry.MangaOriginalUrls;
        for (var i = 0; i < mangaOriginalUrls.Count; ++i)
        {
            var imageDownloadTask = new ImageDownloadTask(new(mangaOriginalUrls[i]), IoHelper.ReplaceTokenExtensionFromUrl(TokenizedDestination, mangaOriginalUrls[i]).Replace(MangaIndexMacro.NameConstToken, i.ToString()));
            AddToTaskSet(imageDownloadTask);
        }
    }

    protected override async Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender)
    {
        if (App.AppViewModel.AppSettings.IllustrationDownloadFormat is IllustrationDownloadFormat.Original)
            return;
        foreach (var destination in Destinations)
            await TagsManager.SetTagsAsync(destination, Entry);
    }
}

public class UgoiraDownloadTaskGroup : DownloadTaskGroup
{
    public Illustration Entry => DatabaseEntry.Entry.To<Illustration>();

    private UgoiraMetadataResponse Metadata { get; }

    private string TempFolderPath => $"{TokenizedDestination}.tmp";

    public UgoiraDownloadTaskGroup(Illustration entry, UgoiraMetadataResponse metadata, string destination) : base(entry, destination, DownloadItemType.Ugoira)
    {
        Metadata = metadata;
        var ugoiraOriginalUrls = entry.GetUgoiraOriginalUrls(metadata.FrameCount);
        _ = Directory.CreateDirectory(TempFolderPath);
        for (var i = 0; i < ugoiraOriginalUrls.Count; ++i)
        {
            var imageDownloadTask = new ImageDownloadTask(new(ugoiraOriginalUrls[i]), $"{TempFolderPath}\\{i}{Path.GetExtension(ugoiraOriginalUrls[i])}");
            AddToTaskSet(imageDownloadTask);
        }
    }

    protected override async Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender)
    {
        if (App.AppViewModel.AppSettings.UgoiraDownloadFormat is UgoiraDownloadFormat.OriginalZip)
        {
            await Task.Yield();
            ZipFile.CreateFromDirectory(TempFolderPath, TokenizedDestination, CompressionLevel.Optimal, false);
            Directory.Delete(TempFolderPath);
        }
        else
        {
            using var image = await Destinations.UgoiraSaveToImageAsync(Metadata.Delays.ToArray());
            image.SetIdTags(Entry);
            await image.SaveAsync(TokenizedDestination, IoHelper.GetUgoiraEncoder());
            Directory.Delete(TempFolderPath);
        }
    }
}

public class NovelDownloadTaskGroup : DownloadTaskGroup
{
    public Novel Entry => DatabaseEntry.Entry.To<Novel>();

    public NovelContent NovelContent { get; }

    public DocumentViewerViewModel DocumentViewModel { get; }

    /// <summary>
    /// 小说正文的保存路径
    /// </summary>
    private string DocPath { get; }

    private string PdfTempFolderPath => $"{TokenizedDestination}.tmp";

    public NovelDownloadTaskGroup(
        Novel entry,
        NovelContent novelContent,
        DocumentViewerViewModel documentViewModel,
        string destination) : base(entry, destination, DownloadItemType.Novel)
    {
        NovelContent = novelContent;
        DocumentViewModel = documentViewModel;
        var backSlash = TokenizedDestination.LastIndexOf('\\');
        // ..\[ID] NovelName.pdf
        // ..\[ID] NovelName\novel.txt
        DocPath = TokenizedDestination[..backSlash];
        // <ext> or .png or .etc 
        var imgExt = TokenizedDestination[(backSlash + 1)..];
        var directory = DocPath.EndsWith(".pdf") ? PdfTempFolderPath : Path.GetDirectoryName(TokenizedDestination)!;

        _ = Directory.CreateDirectory(directory);
        var flag = false;
        if (imgExt == FileExtensionMacro.NameConstToken)
            flag = true;
        else
            DocumentViewModel.ImageExtension = imgExt;
        for (var i = 0; i < DocumentViewModel.TotalCount; ++i)
        {
            var url = DocumentViewModel.AllUrls[i];
            var name = Path.Combine(directory, DocumentViewModel.AllTokens[i]);
            var imageDownloadTask = new ImageDownloadTask(new(url), flag
                ? IoHelper.ReplaceTokenExtensionFromUrl(name, url)
                : name + imgExt);
            AddToTaskSet(imageDownloadTask);
        }
    }

    protected override async Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender)
    {
        for (var i = 0; i < DocumentViewModel.TotalCount; ++i)
        {
            var stream = DocumentViewModel.GetStream(i);
            stream.Position = 0;
        }

        string content;
        switch (App.AppViewModel.AppSettings.NovelDownloadFormat)
        {
            case NovelDownloadFormat.Pdf:
                var document = DocumentViewModel.LoadPdfContent();
                document.GeneratePdf(DocPath);
                Directory.Delete(PdfTempFolderPath);
                return;
            case NovelDownloadFormat.OriginalTxt:
                content = NovelContent.Text;
                break;
            case NovelDownloadFormat.Html:
                content = DocumentViewModel.LoadHtmlContent().ToString();
                break;
            case NovelDownloadFormat.Md:
                content = DocumentViewModel.LoadMdContent().ToString();
                break;
            default:
                ThrowHelper.ArgumentOutOfRange(App.AppViewModel.AppSettings.NovelDownloadFormat);
                return;
        }

        await File.WriteAllTextAsync(DocPath, content);
        if (App.AppViewModel.AppSettings.IllustrationDownloadFormat is IllustrationDownloadFormat.Original)
            return;
        for (var i = 0; i < Destinations.Count; ++i)
        {
            var destination = Destinations[i];
            if (DocumentViewModel.GetIdTags(i) is { Id: var id, Tags: var tags })
                await TagsManager.SetIdTagsAsync(destination, id, tags);
        }
    }
}

public class SingleImageDownloadTaskGroup : ImageDownloadTask, ITaskGroup
{
    public DownloadHistoryEntry DatabaseEntry { get; }

    public Illustration Entry => DatabaseEntry.Entry.To<Illustration>();

    public long Id => DatabaseEntry.Entry.Id;

    protected SingleImageDownloadTaskGroup(Illustration entry, string destination) : base(new(entry.OriginalSingleUrl!), new(IoHelper.ReplaceTokenExtensionFromUrl(destination, entry.OriginalSingleUrl!)))
    {
        DatabaseEntry = new(Destination, DownloadItemType.Illustration, entry);
        PropertyChanged += (sender, e) =>
        {
            var g = sender.To<DownloadTaskGroup>();
            if (e.PropertyName is not nameof(CurrentState))
                return;
            if (g.CurrentState is DownloadState.Running or DownloadState.Paused)
                return;
            g.DatabaseEntry.State = g.CurrentState;
            var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
            manager.Update(g.DatabaseEntry);
        };
    }

    public bool IsCompleted => CurrentState is DownloadState.Completed;

    public bool IsError => CurrentState is DownloadState.Error;

    protected override async Task AfterDownloadAsyncOverride(ImageDownloadTask sender)
    {
        if (App.AppViewModel.AppSettings.IllustrationDownloadFormat is IllustrationDownloadFormat.Original)
            return;
        await TagsManager.SetTagsAsync(Destination, Entry);
    }

    public int Count => 1;

    public IEnumerator<ImageDownloadTask> GetEnumerator() => ((IReadOnlyList<ImageDownloadTask>)[this]).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

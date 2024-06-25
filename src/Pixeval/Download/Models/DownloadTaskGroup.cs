#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/DownloadTaskGroup.cs
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
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.CoreApi.Model;
using Pixeval.Database;
using Pixeval.Database.Managers;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

public abstract class DownloadTaskGroup(DownloadHistoryEntry entry) : ObservableObject, IDownloadTaskGroup
{
    public DownloadHistoryEntry DatabaseEntry { get; } = entry;

    public abstract ValueTask InitializeTaskGroupAsync();

    public long Id => DatabaseEntry.Entry.Id;

    protected DownloadTaskGroup(IWorkEntry entry, string destination, DownloadItemType type) : this(new(destination, type, entry)) => SetNotCreateFromEntry();

    protected void SetNotCreateFromEntry()
    {
        if (!IsCreateFromEntry)
            return;
        IsCreateFromEntry = false;
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

    private bool IsCreateFromEntry { get; set; } = true;

    protected abstract Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender);

    /// <inheritdoc cref="DownloadHistoryEntry.Destination"/>
    public string TokenizedDestination => DatabaseEntry.Destination;

    public int Count => TasksSet.Count;

    public IReadOnlyList<string> Destinations => TasksSet.Select(t => t.Destination).ToArray();

    protected IReadOnlyList<ImageDownloadTask> TasksSet => _tasksSet;

    private readonly List<ImageDownloadTask> _tasksSet = [];

    string IDownloadTaskBase.Destination => DatabaseEntry.Destination;

    public abstract string OpenLocalDestination { get; }

    public void TryReset() => TasksSet.ForEach(t => t.TryReset());

    public void Pause() => TasksSet.ForEach(t => t.Pause());

    public void TryResume() => TasksSet.ForEach(t => t.TryResume());

    public void Cancel() => TasksSet.ForEach(t => t.Cancel());

    public abstract void Delete();

    public DownloadState CurrentState
    {
        get
        {
            if (IsCreateFromEntry)
                return DatabaseEntry.State;
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

    protected void AddToTasksSet(ImageDownloadTask task)
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

    public double ProgressPercentage => TasksSet.Count is 0 ? 100 : TasksSet.Average(t => t.ProgressPercentage);

    public IEnumerator<ImageDownloadTask> GetEnumerator() => TasksSet.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var task in TasksSet)
            task.Dispose();
    }
}

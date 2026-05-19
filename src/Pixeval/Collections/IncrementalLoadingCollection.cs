// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.Collections;

/// <summary>
/// This class represents an <see cref="ObservableCollection{IType}"/> whose items can be loaded incrementally.
/// </summary>
/// <typeparam name="IType">
/// The type of collection items.
/// </typeparam>
/// <seealso cref="IIncrementalSource{TSource}"/>
/// <seealso cref="IIncrementalLoading"/>
public class IncrementalLoadingCollection<IType> : ObservableCollection<IType>, IIncrementalLoading
{
    private readonly Lock _loadingTaskGate = new();
    private Task<int>? _loadingTask;

    /// <summary>
    /// Gets or sets an <see cref="Action"/> that is called when a retrieval operation begins.
    /// </summary>
    public event Action? OnStartLoading;

    /// <summary>
    /// Gets or sets an <see cref="Action"/> that is called when a retrieval operation ends.
    /// </summary>
    public event Action? OnEndLoading;

    /// <summary>
    /// Gets or sets an <see cref="Action"/> that is called if an error occurs during data retrieval. The actual <see cref="Exception"/> is passed as an argument.
    /// </summary>
    public event Action<Exception>? OnError;

    /// <summary>
    /// Gets a value indicating the source of incremental loading.
    /// </summary>
    protected IIncrementalSource<IType> Source { get; }

    /// <summary>
    /// Gets a value indicating how many items that must be retrieved for each incremental call.
    /// </summary>
    protected int ItemsPerPage { get; }

    /// <summary>
    /// Gets or sets a value indicating The zero-based index of the current items page.
    /// </summary>
    protected int CurrentPageIndex { get; set; }

    /// <summary>
    /// Gets a value indicating whether new items are being loaded.
    /// </summary>
    public bool IsLoading
    {
        get;
        private set
        {
            if (value == field)
                return;
            field = value;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsLoading)));

            if (field)
            {
                OnStartLoading?.Invoke();
            }
            else
            {
                OnEndLoading?.Invoke();
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the collection contains more items to retrieve.
    /// </summary>
    public bool HasMoreItems
    {
        get;

        private set
        {
            if (value != field)
            {
                field = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasMoreItems)));
            }
        }
    } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="IncrementalLoadingCollection{IType}"/> class using the specified <see cref="IIncrementalSource{TSource}"/> implementation and, optionally, how many items to load for each data page.
    /// </summary>
    /// <param name="source">
    /// An implementation of the <see cref="IIncrementalSource{TSource}"/> interface that contains the logic to actually load data incrementally.
    /// </param>
    /// <param name="itemsPerPage">
    /// The number of items to retrieve for each call. Default is 20.
    /// </param>
    /// <param name="onStartLoading">
    /// An <see cref="Action"/> that is called when a retrieval operation begins.
    /// </param>
    /// <param name="onEndLoading">
    /// An <see cref="Action"/> that is called when a retrieval operation ends.
    /// </param>
    /// <param name="onError">
    /// An <see cref="Action"/> that is called if an error occurs during data retrieval.
    /// </param>
    /// <seealso cref="IIncrementalSource{TSource}"/>

    public IncrementalLoadingCollection(IIncrementalSource<IType> source, int itemsPerPage = 20, Action? onStartLoading = null, Action? onEndLoading = null, Action<Exception>? onError = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        Source = source;

        OnStartLoading += onStartLoading;
        OnEndLoading += onEndLoading;
        OnError += onError;

        ItemsPerPage = itemsPerPage;
    }


    /// <summary>
    /// Clears the collection and triggers/forces a reload of the first page
    /// </summary>
    /// <param name="token">
    /// Used to propagate notification that operation should be canceled.
    /// </param>
    /// <returns>This method does not return a result</returns>
    public async Task RefreshAsync(CancellationToken token = default)
    {
        var loadingTask = GetRunningLoadingTask();
        if (loadingTask is not null)
        {
            try
            {
                await WaitForLoadingTaskAsync(loadingTask, token);
            }
            catch (OperationCanceledException) when (!token.IsCancellationRequested)
            {
                // The in-flight load was canceled independently; the refresh can still continue.
            }
        }

        token.ThrowIfCancellationRequested();

        var previousCount = Count;
        Clear();
        CurrentPageIndex = 0;
        HasMoreItems = true;

        if (previousCount is 0)
        {
            // When the list was empty before clearing, the automatic reload isn't fired, so force a reload.
            await LoadMoreItemsAsync(0, token);
        }
    }

    /// <summary>
    /// Actually performs the incremental loading.
    /// </summary>
    /// <param name="token">
    /// Used to propagate notification that operation should be canceled.
    /// </param>
    /// <returns>
    /// Returns a collection of <typeparamref name="IType"/>.
    /// </returns>
    protected virtual async Task<IReadOnlyCollection<IType>> LoadDataAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var result = await Source.GetPagedItemsAsync(CurrentPageIndex, ItemsPerPage, token);
        token.ThrowIfCancellationRequested();
        CurrentPageIndex += 1;
        return result;
    }

    /// <summary>
    /// Initializes incremental loading from the view.
    /// </summary>
    /// <param name="count">
    /// The number of items to load.
    /// </param>
    /// <param name="token"></param>
    /// <returns>
    /// How many items have been actually retrieved.
    /// </returns>
    public Task<int> LoadMoreItemsAsync(int count, CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled<int>(token);

        lock (_loadingTaskGate)
        {
            if (_loadingTask is { IsCompleted: false } loadingTask)
                return WaitForLoadingTaskAsync(loadingTask, token);

            var newLoadingTask = LoadMoreItemsCoreAsync(token);
            _loadingTask = newLoadingTask;
            _ = ResetLoadingTaskAsync(newLoadingTask);
            return newLoadingTask;
        }
    }

    private async Task<int> LoadMoreItemsCoreAsync(CancellationToken token)
    {
        try
        {
            IReadOnlyCollection<IType> data;
            try
            {
                IsLoading = true;
                data = await LoadDataAsync(token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                OnError?.Invoke(ex);
                HasMoreItems = false;
                return 0;
            }

            if (data is not { Count: not 0 })
            {
                HasMoreItems = false;
                return 0;
            }

            foreach (var item in data)
            {
                Add(item);
            }

            return data.Count;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Task<int>? GetRunningLoadingTask()
    {
        lock (_loadingTaskGate)
        {
            return _loadingTask is { IsCompleted: false } loadingTask ? loadingTask : null;
        }
    }

    private static Task<int> WaitForLoadingTaskAsync(Task<int> loadingTask, CancellationToken token)
    {
        return token.CanBeCanceled ? loadingTask.WaitAsync(token) : loadingTask;
    }

    private async Task ResetLoadingTaskAsync(Task<int> loadingTask)
    {
        try
        {
            await loadingTask;
        }
        catch
        {
            // The initiating caller observes the load failure; this continuation only resets shared state.
        }
        finally
        {
            lock (_loadingTaskGate)
            {
                if (ReferenceEquals(_loadingTask, loadingTask))
                    _loadingTask = null;
            }
        }
    }
}

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
/// <typeparam name="TSource">
/// The data source that must be loaded incrementally.
/// </typeparam>
/// <typeparam name="IType">
/// The type of collection items.
/// </typeparam>
/// <seealso cref="IIncrementalSource{TSource}"/>
/// <seealso cref="ISupportIncrementalLoading"/>
public class IncrementalLoadingCollection<TSource, IType> : ObservableCollection<IType>, ISupportIncrementalLoading
    where TSource : IIncrementalSource<IType>
{
    private readonly SemaphoreSlim _mutex = new SemaphoreSlim(1);

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
    protected TSource Source { get; }

    /// <summary>
    /// Gets a value indicating how many items that must be retrieved for each incremental call.
    /// </summary>
    protected int ItemsPerPage { get; }

    /// <summary>
    /// Gets or sets a value indicating The zero-based index of the current items page.
    /// </summary>
    protected int CurrentPageIndex { get; set; }

    private CancellationToken _cancellationToken;
    private bool _refreshOnLoad;

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
        get => !_cancellationToken.IsCancellationRequested && field;

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
    /// Initializes a new instance of the <see cref="IncrementalLoadingCollection{TSource, IType}"/> class using the specified <see cref="IIncrementalSource{TSource}"/> implementation and, optionally, how many items to load for each data page.
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

    public IncrementalLoadingCollection(TSource source, int itemsPerPage = 20, Action? onStartLoading = null, Action? onEndLoading = null, Action<Exception>? onError = null)
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
    /// <returns>This method does not return a result</returns>
    public Task RefreshAsync()
    {
        if (IsLoading)
        {
            _refreshOnLoad = true;
        }
        else
        {
            var previousCount = Count;
            Clear();
            CurrentPageIndex = 0;
            HasMoreItems = true;

            if (previousCount == 0)
            {
                // When the list was empty before clearing, the automatic reload isn't fired, so force a reload.
                return LoadMoreItemsAsync(0);
            }
        }

        return Task.CompletedTask;
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
        var result = await Source.GetPagedItemsAsync(CurrentPageIndex, ItemsPerPage, token);
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
    public async Task<int> LoadMoreItemsAsync(int count, CancellationToken token = default)
    {
        var resultCount = 0;
        _cancellationToken = token;

        await _mutex.WaitAsync(token);
        try
        {
            if (!_cancellationToken.IsCancellationRequested)
            {
                IReadOnlyCollection<IType>? data = null;
                try
                {
                    IsLoading = true;
                    data = await LoadDataAsync(_cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // The operation has been canceled using the Cancellation Token.
                }
                catch (Exception ex) when (OnError is not null)
                {
                    OnError(ex);
                }

                if (data is { Count: not 0 } && !_cancellationToken.IsCancellationRequested)
                {
                    resultCount = data.Count;

                    foreach (var item in data)
                    {
                        Add(item);
                    }
                }
                else
                {
                    HasMoreItems = false;
                }
            }
        }
        finally
        {
            IsLoading = false;

            if (_refreshOnLoad)
            {
                _refreshOnLoad = false;
                await RefreshAsync();
            }

            _mutex.Release();
        }

        return resultCount;
    }
}

// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mako.Engine;
using Misaki;
using Pixeval.Collections;

namespace Pixeval.ViewModels;

public class IncrementalSource<T, TViewModel> : IIncrementalSource<TViewModel>, IDisposable
    where T : IIdentityInfo
{
    private readonly IAsyncEnumerable<T?> _asyncEnumerable;

    private readonly Func<T, int, TViewModel> _factory;

    private readonly int _limit;

    private readonly CancellationTokenSource _lifetimeCts = new();

    private readonly Lock _lifetimeGate = new();

    private readonly IAsyncEnumerator<T?> _asyncEnumerator;

    private readonly HashSet<string> _yieldedItems = [];

    private int _yieldedCounter;

    private int _activeRequests;

    private bool _isDisposed;

    private bool _resourcesDisposed;

    public IncrementalSource(IAsyncEnumerable<T?> asyncEnumerable, Func<T, int, TViewModel> factory, int limit = -1)
    {
        ArgumentNullException.ThrowIfNull(asyncEnumerable);
        ArgumentNullException.ThrowIfNull(factory);
        _asyncEnumerable = asyncEnumerable;
        _factory = factory;
        _limit = limit;
        _asyncEnumerator = _asyncEnumerable.GetAsyncEnumerator(_lifetimeCts.Token);
    }

    public virtual async Task<IReadOnlyCollection<TViewModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        BeginRequest();
        try
        {
            var result = new List<TViewModel>(pageSize);
            var i = 0;
            while (i < pageSize)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (_limit is not -1 && _yieldedCounter >= _limit)
                {
                    return result;
                }

                if (await _asyncEnumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (_asyncEnumerator.Current is { } obj && !_yieldedItems.Contains(Identifier(obj)))
                    {
                        result.Add(_factory(obj, _yieldedCounter));
                        _ = _yieldedItems.Add(Identifier(obj));
                        ++i;
                        _yieldedCounter++;
                    }
                }
                else
                {
                    return result;
                }
            }

            return result;
        }
        finally
        {
            EndRequest();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        bool disposeResources;
        lock (_lifetimeGate)
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            disposeResources = _activeRequests is 0;
            _resourcesDisposed = disposeResources;
        }

        _lifetimeCts.Cancel();
        if (_asyncEnumerable is IEngineHandleSource { EngineHandle: { } engineHandle })
            engineHandle.Cancel();

        if (disposeResources)
            DisposeResources();
    }

    private void BeginRequest()
    {
        lock (_lifetimeGate)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            _activeRequests++;
        }
    }

    private void EndRequest()
    {
        bool disposeResources;
        lock (_lifetimeGate)
        {
            _activeRequests--;
            disposeResources = _isDisposed && _activeRequests is 0 && !_resourcesDisposed;
            if (disposeResources)
                _resourcesDisposed = true;
        }

        if (disposeResources)
            DisposeResources();
    }

    private void DisposeResources()
    {
        try
        {
            _asyncEnumerator.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
            // Cancellation is the expected result when a page is closed during loading.
        }
        finally
        {
            _lifetimeCts.Dispose();
        }
    }

    protected virtual string Identifier(T entity) => entity.Id;
}

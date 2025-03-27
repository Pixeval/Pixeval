// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Collections;
using Mako.Engine;
using Mako.Model;

namespace Pixeval.Controls;

public class FetchEngineIncrementalSource<T, TViewModel>(IAsyncEnumerable<T?> asyncEnumerator, int limit = -1)
    : IIncrementalSource<TViewModel>, IIncrementalSourceFactory<T, FetchEngineIncrementalSource<T, TViewModel>>
    where T : IIdEntry
    where TViewModel : IFactory<T, TViewModel>
{
    public static FetchEngineIncrementalSource<T, TViewModel> CreateInstance(IFetchEngine<T> fetchEngine, int limit = -1) => new(fetchEngine, limit);

    /// <summary>
    /// 当为null时暂时不报错
    /// </summary>
    private readonly IAsyncEnumerator<T> _asyncEnumerator = asyncEnumerator?.GetAsyncEnumerator()!;

    private readonly HashSet<long> _yieldedItems = [];

    private int _yieldedCounter;

    public virtual async Task<IEnumerable<TViewModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = new List<TViewModel>();
        var i = 0;
        while (i < pageSize)
        {
            if (limit is not -1 && _yieldedCounter >= limit)
            {
                return result;
            }

            if (await _asyncEnumerator.MoveNextAsync())
            {
                if (_asyncEnumerator.Current is { } obj && !_yieldedItems.Contains(Identifier(obj)))
                {
                    result.Add(Select(obj, _yieldedCounter));
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

    protected virtual long Identifier(T entity) => entity.Id;

    protected TViewModel Select(T entity, int index) => TViewModel.CreateInstance(entity);
}

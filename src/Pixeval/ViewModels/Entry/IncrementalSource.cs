// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Misaki;
using Pixeval.Collections;

namespace Pixeval.ViewModels;

public class IncrementalSource<T, TViewModel>(IAsyncEnumerable<T?> asyncEnumerator, Func<T, int, TViewModel> factory, int limit = -1)
    : IIncrementalSource<TViewModel>
    where T : IIdentityInfo
{
    /// <summary>
    /// 当为null时暂时不报错
    /// </summary>
    private readonly IAsyncEnumerator<T?> _asyncEnumerator = asyncEnumerator.GetAsyncEnumerator();

    private readonly HashSet<string> _yieldedItems = [];

    private int _yieldedCounter;

    public virtual async Task<IReadOnlyCollection<TViewModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = new List<TViewModel>(pageSize);
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
                    result.Add(factory(obj, _yieldedCounter));
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

    protected virtual string Identifier(T entity) => entity.Id;
}

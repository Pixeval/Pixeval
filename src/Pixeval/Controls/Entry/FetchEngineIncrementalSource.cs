#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/FetchEngineIncrementalSource.cs
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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Collections;

namespace Pixeval.Controls;

public abstract class FetchEngineIncrementalSource<T, TModel>(IAsyncEnumerable<T?> asyncEnumerator, int limit = -1)
    : IIncrementalSource<TModel>
{
    /// <summary>
    /// 当为null时暂时不报错
    /// </summary>
    private readonly IAsyncEnumerator<T> _asyncEnumerator = asyncEnumerator?.GetAsyncEnumerator()!;

    private readonly ISet<long> _yieldedItems = new HashSet<long>();

    private int _yieldedCounter;

    public virtual async Task<IEnumerable<TModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = new CancellationToken())
    {
        var result = new List<TModel>();
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
                    result.Add(Select(obj));
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

    protected abstract long Identifier(T entity);

    protected abstract TModel Select(T entity);
}

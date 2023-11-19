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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Collections;

namespace Pixeval.Misc;

public abstract class FetchEngineIncrementalSource<T, TModel>(IAsyncEnumerable<T> asyncEnumerator, int limit = -1)
    : IIncrementalSource<TModel>
{
    private readonly ISet<long> _yieldedItems = new HashSet<long>();

    private readonly IAsyncEnumerator<T> _asyncEnumerator = asyncEnumerator.GetAsyncEnumerator();

    private int _yieldedCounter;

    protected abstract long Identifier(T entity);

    protected abstract TModel Select(T entity);

    public async Task<IEnumerable<TModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = new())
    {
        var result = new List<TModel>();
        var i = 0;
        while (i < pageSize)
        {
            if (limit is not -1 && _yieldedCounter > limit)
            {
                return result;
            }
            if (await _asyncEnumerator.MoveNextAsync())
            {
                if (_asyncEnumerator.Current is { } obj && !_yieldedItems.Contains(Identifier(obj)))
                {
                    result.Add(Select(obj));
                    _ = _yieldedItems.Add(Identifier(obj));
                    i++;
                    _yieldedCounter++;
                }
            }
            else
            {
                return Enumerable.Empty<TModel>();
            }
        }

        return result;
    }
}

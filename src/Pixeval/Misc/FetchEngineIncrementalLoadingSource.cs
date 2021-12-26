#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/FetchEngineIncrementalLoadingSource.cs
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
using CommunityToolkit.Common.Collections;

namespace Pixeval.Misc;

public class FetchEngineIncrementalLoadingSource<TEntity> : IIncrementalSource<TEntity>
{
    private readonly IAsyncEnumerator<TEntity> _asyncEnumerator;

    public FetchEngineIncrementalLoadingSource(IAsyncEnumerable<TEntity> fetchEngine)
    {
        _asyncEnumerator = fetchEngine.GetAsyncEnumerator();
    }

    public async Task<IEnumerable<TEntity>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = new())
    {
        var result = new List<TEntity>();
        for (var i = 0; i < pageSize; i++)
        {
            if (await _asyncEnumerator.MoveNextAsync())
            {
                result.Add(_asyncEnumerator.Current);
            }
            else
            {
                break;
            }
        }

        return result;
    }
}
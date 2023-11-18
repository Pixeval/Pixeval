#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/FetchEngineSelector.cs
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.CoreApi.Engine;

internal class FetchEngineSelector<T, R>(IFetchEngine<T> delegateEngine, Func<T, Task<R>> selector) : IFetchEngine<R>
{
    public MakoClient MakoClient { get; } = delegateEngine.MakoClient;

    public EngineHandle EngineHandle { get; } = delegateEngine.EngineHandle;

    public int RequestedPages { get; set; } = 0;

    public IAsyncEnumerator<R> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new FetchEngineSelectorAsyncEnumerator(delegateEngine.GetAsyncEnumerator(cancellationToken), selector)!;
    }

    private class FetchEngineSelectorAsyncEnumerator(IAsyncEnumerator<T> delegateEnumerator, Func<T, Task<R>> selector)
        : IAsyncEnumerator<R?>
    {
        public ValueTask DisposeAsync()
        {
            return delegateEnumerator.DisposeAsync();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (await delegateEnumerator.MoveNextAsync().ConfigureAwait(false))
            {
                Current = await selector(delegateEnumerator.Current).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        public R? Current { get; private set; }
    }
}
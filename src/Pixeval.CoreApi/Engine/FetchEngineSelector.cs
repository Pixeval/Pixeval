#region Copyright (c) Pixeval/Pixeval.CoreApi

// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/FetchEngineSelector.cs
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

internal class FetchEngineSelector<T, R> : IFetchEngine<R>
{
    private readonly IFetchEngine<T> _delegateEngine;
    private readonly Func<T, Task<R>> _selector;

    public FetchEngineSelector(IFetchEngine<T> delegateEngine, Func<T, Task<R>> selector)
    {
        _delegateEngine = delegateEngine;
        _selector = selector;
        MakoClient = delegateEngine.MakoClient;
        EngineHandle = delegateEngine.EngineHandle;
        RequestedPages = 0;
    }

    public MakoClient MakoClient { get; }

    public EngineHandle EngineHandle { get; }

    public int RequestedPages { get; set; }

    public IAsyncEnumerator<R> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new FetchEngineSelectorAsyncEnumerator(_delegateEngine.GetAsyncEnumerator(cancellationToken), _selector)!;
    }

    private class FetchEngineSelectorAsyncEnumerator : IAsyncEnumerator<R?>
    {
        private readonly IAsyncEnumerator<T> _delegateEnumerator;
        private readonly Func<T, Task<R>> _selector;

        public FetchEngineSelectorAsyncEnumerator(IAsyncEnumerator<T> delegateEnumerator, Func<T, Task<R>> selector)
        {
            _delegateEnumerator = delegateEnumerator;
            _selector = selector;
        }

        public ValueTask DisposeAsync()
        {
            return _delegateEnumerator.DisposeAsync();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (await _delegateEnumerator.MoveNextAsync().ConfigureAwait(false))
            {
                Current = await _selector(_delegateEnumerator.Current).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        public R? Current { get; private set; }
    }
}
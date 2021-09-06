#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/FetchEngineSelector.cs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.CoreApi.Engine
{
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
}
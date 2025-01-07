// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

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

    public IAsyncEnumerator<R> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
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

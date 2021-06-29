using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.CoreApi.Engines
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
                if (await _delegateEnumerator.MoveNextAsync())
                {
                    Current = await _selector(_delegateEnumerator.Current);
                    return true;
                }

                return false;
            }

            public R? Current { get; private set; }
        }
    }
}
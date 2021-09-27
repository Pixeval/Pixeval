using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Pixeval.Utilities
{
    [PublicAPI]
    public class AdaptedAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly CancellationToken _cancellationToken;
        private readonly IEnumerator<T> _outerEnumerator;

        public AdaptedAsyncEnumerator(IEnumerator<T> outerEnumerator, CancellationToken cancellationToken = new())
        {
            _outerEnumerator = outerEnumerator;
            _cancellationToken = cancellationToken;
        }

        public ValueTask DisposeAsync()
        {
            _outerEnumerator.Dispose();
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return ValueTask.FromResult(!_cancellationToken.IsCancellationRequested && _outerEnumerator.MoveNext());
        }

        public T Current => _outerEnumerator.Current;
    }

    [PublicAPI]
    public class AdaptedAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly IEnumerable<T> _sync;

        public AdaptedAsyncEnumerable(IEnumerable<T> sync)
        {
            _sync = sync;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            return new AdaptedAsyncEnumerator<T>(_sync.GetEnumerator());
        }
    }
}
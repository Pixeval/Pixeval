using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixeval.Core
{
    public abstract class AbstractPixivAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        protected IPixivAsyncEnumerable<T> Enumerable;

        protected AbstractPixivAsyncEnumerator(IPixivAsyncEnumerable<T> enumerable)
        {
            Enumerable = enumerable;
        }

        public ValueTask DisposeAsync()
        {
            return default;
        }

        protected abstract void UpdateEnumerator();

        public abstract ValueTask<bool> MoveNextAsync();

        public abstract T Current { get; }
    }
}